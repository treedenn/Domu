using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Spaces.Application.Expirations.Contracts;
using Domu.Api.Features.Spaces.Application.Expirations.Ports;

namespace Domu.Api.Features.Spaces.Application.Expirations;

public sealed class GetHouseholdExpirationsUseCase(
    IHouseholdExpirationQueryService expirationQueryService,
    IHouseholdAccessService householdAccessService,
    TimeProvider timeProvider)
{
    private const int MaximumUpcomingDays = 30;

    public async Task<HouseholdExpirationsView> ExecuteAsync(
        GetHouseholdExpirationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var evaluatedAtUtc = timeProvider.GetUtcNow();
        var upcomingUntilUtc = query.UpcomingUntilUtc.ToUniversalTime();
        if (upcomingUntilUtc < evaluatedAtUtc)
            throw new ArgumentOutOfRangeException(nameof(query.UpcomingUntilUtc),
                "Upcoming expiry range cannot end before the evaluation time.");
        if (upcomingUntilUtc > evaluatedAtUtc.AddDays(MaximumUpcomingDays))
            throw new ArgumentOutOfRangeException(nameof(query.UpcomingUntilUtc),
                "Upcoming expiry range cannot extend more than 30 days ahead.");

        await householdAccessService.EnsureCanAccessHouseholdAsync(
            query.Actor,
            query.HouseholdId,
            cancellationToken);

        var batches = await expirationQueryService.GetAsync(
            query.HouseholdId,
            upcomingUntilUtc,
            cancellationToken);

        var expired = batches
            .Where(batch => batch.ExpirationDate < evaluatedAtUtc)
            .OrderBy(batch => batch.ExpirationDate)
            .ThenBy(batch => batch.EntryId)
            .ToArray();
        var upcoming = batches
            .Where(batch => batch.ExpirationDate >= evaluatedAtUtc && batch.ExpirationDate <= upcomingUntilUtc)
            .OrderBy(batch => batch.ExpirationDate)
            .ThenBy(batch => batch.EntryId)
            .ToArray();

        return new HouseholdExpirationsView(evaluatedAtUtc, expired, upcoming);
    }
}
