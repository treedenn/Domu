using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Activities.Domain;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Activities.Infrastructure;

public sealed class HouseholdActivityQueryService(AppDbContext dbContext) : IHouseholdActivityQueryService
{
    public async Task<IReadOnlyList<HouseholdActivity>> GetRecentHouseholdActivitiesAsync(
        Guid householdId,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        var activities = await dbContext.HouseholdActivities
            .AsNoTracking()
            .Where(householdActivity => householdActivity.HouseholdId == householdId && householdActivity.OccurredAt >= since)
            .OrderByDescending(householdActivity => householdActivity.OccurredAt)
            .ToListAsync(cancellationToken);

        return activities.Select(householdActivity => householdActivity.ToDomain()).ToArray();
    }
}