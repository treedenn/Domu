using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Events.Application;

public interface IHouseholdEventQueryService
{
    Task<IReadOnlyList<HouseholdEvent>> GetRecentHouseholdEventsAsync(
        Guid householdId,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}