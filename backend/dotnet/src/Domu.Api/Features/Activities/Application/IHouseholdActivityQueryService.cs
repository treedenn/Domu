using Domu.Api.Features.Activities.Domain;

namespace Domu.Api.Features.Activities.Application;

public interface IHouseholdActivityQueryService
{
    Task<IReadOnlyList<HouseholdActivity>> GetRecentHouseholdActivitiesAsync(
        Guid householdId,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}