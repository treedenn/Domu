using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Events.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Insights.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Tests.Features.Insights.Application;

public sealed class GetHouseholdInsightsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeduplicatesCandidatesByHighestScore()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var queryService = new FakeUserEventQueryService();
        var useCase = new GetHouseholdInsightsUseCase(
            new FakeHouseholdAccessService(),
            queryService,
            [
                new StaticInsightRule("low", "same-target", 0.5),
                new StaticInsightRule("high", "same-target", 0.9)
            ]);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdInsightsQuery(householdId, userId),
            CancellationToken.None);

        var insight = Assert.Single(result.Insights);
        Assert.Equal("high", insight.CreatedFrom);
        Assert.Equal(0.9, insight.Score);
        Assert.Equal(householdId, queryService.RequestedHouseholdId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccessFails_DoesNotQueryEvents()
    {
        var queryService = new FakeUserEventQueryService();
        var useCase = new GetHouseholdInsightsUseCase(
            new FakeHouseholdAccessService { DenyAccess = true },
            queryService,
            [new StaticInsightRule("rule", "dedupe", 0.5)]);

        var action = () => useCase.ExecuteAsync(
            new GetHouseholdInsightsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal(0, queryService.Calls);
    }

    private sealed class FakeHouseholdAccessService : IHouseholdAccessService
    {
        public bool DenyAccess { get; set; }

        public Task EnsureCanAccessHouseholdAsync(
            Guid householdId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (DenyAccess)
                throw new KeyNotFoundException();

            return Task.CompletedTask;
        }

        public Task<Guid> GetRequiredMemberIdAsync(
            Guid householdId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (DenyAccess)
                throw new KeyNotFoundException();

            return Task.FromResult(Guid.NewGuid());
        }
    }

    private sealed class FakeUserEventQueryService : IUserEventQueryService
    {
        public int Calls { get; private set; }
        public Guid? RequestedHouseholdId { get; private set; }

        public Task<IReadOnlyList<UserEvent>> GetRecentHouseholdEventsAsync(
            Guid householdId,
            DateTimeOffset since,
            CancellationToken cancellationToken)
        {
            Calls++;
            RequestedHouseholdId = householdId;
            return Task.FromResult<IReadOnlyList<UserEvent>>([]);
        }
    }

    private sealed class StaticInsightRule(string key, string dedupeKey, double score) : IInsightRule
    {
        public string Key => key;

        public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
            InsightContext context,
            CancellationToken cancellationToken)
        {
            var insight = new HouseholdInsight(
                key,
                "test.insight",
                "Title",
                "Body",
                score,
                1,
                key,
                "household",
                context.HouseholdId,
                null,
                new Dictionary<string, object?>());

            return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>(
                [new HouseholdInsightCandidate(dedupeKey, insight)]);
        }
    }
}
