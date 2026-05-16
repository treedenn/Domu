using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Events.Application;

public interface IUserEventQueryService
{
    Task<IReadOnlyList<UserEvent>> GetRecentHouseholdEventsAsync(
        Guid householdId,
        DateTimeOffset since,
        CancellationToken cancellationToken);
}
