using Domu.Api.Features.Locations.Application.Items.Ports;
using Domu.Api.Features.Locations.Domain.Items;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Locations.Infrastructure.Items;

public sealed class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Items
            .Include(item => item.Entries)
            .SingleOrDefaultAsync(item => item.Id == itemId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Item>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var entities = await dbContext.Items
            .AsNoTracking()
            .Include(item => item.Entries)
            .Where(item => item.LocationId == locationId)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);

        return entities
            .Select(item => item.ToDomain())
            .ToArray();
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken)
    {
        await dbContext.Items.AddAsync(ItemEntity.FromDomain(item), cancellationToken);
    }

    public async Task UpdateAsync(Item item, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Items
            .Include(existingItem => existingItem.Entries)
            .SingleOrDefaultAsync(existingItem => existingItem.Id == item.Id, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Item '{item.Id}' was not found.");

        existingEntity.UpdateFromDomain(item);
    }

    public async Task DeleteAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Items
            .SingleOrDefaultAsync(item => item.Id == itemId, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Item '{itemId}' was not found.");

        dbContext.Items.Remove(existingEntity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
