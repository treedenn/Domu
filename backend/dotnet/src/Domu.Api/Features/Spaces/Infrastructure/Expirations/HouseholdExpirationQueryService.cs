using Domu.Api.Features.Spaces.Application.Expirations.Contracts;
using Domu.Api.Features.Spaces.Application.Expirations.Ports;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Spaces.Infrastructure.Expirations;

public sealed class HouseholdExpirationQueryService(AppDbContext dbContext) : IHouseholdExpirationQueryService
{
    public async Task<IReadOnlyList<ExpirationBatchView>> GetAsync(
        Guid householdId,
        DateTimeOffset untilUtc,
        CancellationToken cancellationToken)
    {
        return await (
                from entry in dbContext.ItemEntries.AsNoTracking()
                join item in dbContext.Items.AsNoTracking() on entry.ItemId equals item.Id
                join space in dbContext.Spaces.AsNoTracking() on item.SpaceId equals space.Id
                where space.HouseholdId == householdId
                      && entry.ExpirationDate != null
                      && entry.ExpirationDate <= untilUtc
                select new ExpirationBatchView(
                    entry.Id,
                    entry.Count,
                    entry.OriginalAmountPerUnit,
                    entry.CurrentAmountPerUnit,
                    entry.Unit,
                    entry.State,
                    entry.AcquisitionDate,
                    entry.ExpirationDate.GetValueOrDefault(),
                    item.Id,
                    item.Name,
                    space.Id,
                    space.Name))
            .ToArrayAsync(cancellationToken);
    }
}
