using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Expirations;
using Domu.Api.Features.Spaces.Application.Expirations.Contracts;
using Domu.Api.Features.Spaces.Application.Expirations.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class GetHouseholdExpirationsUseCaseTests
{
    private static readonly DateTimeOffset EvaluationTime = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_SeparatesAndSortsExpiredAndUpcomingBatches()
    {
        var householdId = Guid.NewGuid();
        var queryService = new FakeExpirationQueryService(
            Batch(EvaluationTime.AddHours(2), "Upcoming later"),
            Batch(EvaluationTime.AddHours(-2), "Expired later"),
            Batch(EvaluationTime, "Boundary"),
            Batch(EvaluationTime.AddHours(-4), "Expired earlier"),
            Batch(EvaluationTime.AddHours(1), "Upcoming earlier"),
            Batch(EvaluationTime.AddDays(31), "Outside range"));
        var useCase = new GetHouseholdExpirationsUseCase(
            queryService,
            new FakeHouseholdAccessService(),
            new FixedTimeProvider(EvaluationTime));

        var result = await useCase.ExecuteAsync(
            Query(householdId, EvaluationTime.AddDays(7)),
            CancellationToken.None);

        Assert.Equal(EvaluationTime, result.EvaluatedAtUtc);
        Assert.Equal(["Expired earlier", "Expired later"], result.Expired.Select(batch => batch.ItemName));
        Assert.Equal(["Boundary", "Upcoming earlier", "Upcoming later"], result.Upcoming.Select(batch => batch.ItemName));
        Assert.Empty(result.Expired.IntersectBy(result.Upcoming.Select(batch => batch.EntryId), batch => batch.EntryId));
        Assert.DoesNotContain(result.Upcoming, batch => batch.ItemName == "Outside range");
        Assert.Equal(householdId, queryService.RequestedHouseholdId);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRangesBeforeEvaluationOrMoreThanThirtyDaysAhead()
    {
        var useCase = new GetHouseholdExpirationsUseCase(
            new FakeExpirationQueryService(),
            new FakeHouseholdAccessService(),
            new FixedTimeProvider(EvaluationTime));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.ExecuteAsync(
            Query(Guid.NewGuid(), EvaluationTime.AddTicks(-1)), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.ExecuteAsync(
            Query(Guid.NewGuid(), EvaluationTime.AddDays(30).AddTicks(1)), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesInaccessibleHouseholdAsNotFound()
    {
        var access = new FakeHouseholdAccessService { DenyAccess = true };
        var useCase = new GetHouseholdExpirationsUseCase(
            new FakeExpirationQueryService(), access, new FixedTimeProvider(EvaluationTime));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => useCase.ExecuteAsync(
            Query(Guid.NewGuid(), EvaluationTime.AddDays(1)), CancellationToken.None));
    }

    private static GetHouseholdExpirationsQuery Query(Guid householdId, DateTimeOffset upcomingUntilUtc) =>
        new(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), householdId, upcomingUntilUtc);

    private static ExpirationBatchView Batch(DateTimeOffset expirationDate, string itemName) =>
        new(Guid.NewGuid(), 2, 1, 0.5m, ItemUnit.Liter, ConsumableState.Opened,
            EvaluationTime.AddDays(-1), expirationDate, Guid.NewGuid(), itemName, Guid.NewGuid(), "Fridge");

    private sealed class FakeExpirationQueryService(params ExpirationBatchView[] batches) : IHouseholdExpirationQueryService
    {
        public Guid RequestedHouseholdId { get; private set; }

        public Task<IReadOnlyList<ExpirationBatchView>> GetAsync(Guid householdId, DateTimeOffset untilUtc,
            CancellationToken cancellationToken)
        {
            RequestedHouseholdId = householdId;
            IReadOnlyList<ExpirationBatchView> result = batches;
            return Task.FromResult(result);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
