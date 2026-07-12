using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Events.Domain;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class HouseholdEventQueryService(AppDbContext dbContext) : IHouseholdEventQueryService
{
    public async Task<IReadOnlyList<HouseholdEvent>> GetRecentHouseholdEventsAsync(
        Guid householdId,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        var events = await dbContext.HouseholdEvents
            .AsNoTracking()
            .Where(userEvent => userEvent.HouseholdId == householdId && userEvent.OccurredAt >= since)
            .OrderByDescending(userEvent => userEvent.OccurredAt)
            .ToListAsync(cancellationToken);

        return events.Select(userEvent => userEvent.ToDomain()).ToArray();
    }
}
