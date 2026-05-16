using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.Items;

public sealed class ShoppingListItemRepository(AppDbContext dbContext) : IShoppingListItemRepository
{
    public async Task<IReadOnlyList<ShoppingListItem>> GetItemsAsync(
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        var entities = await dbContext.ShoppingListItems
            .AsNoTracking()
            .Where(item => item.ShoppingListId == shoppingListId)
            .OrderBy(item => item.Checked)
            .ThenBy(item => item.Checked ? (decimal?)null : item.SortOrder)
            .ThenBy(item => item.Checked ? (DateTimeOffset?)null : item.CreatedAt)
            .ThenByDescending(item => item.Checked ? item.CheckedAt : null)
            .ThenByDescending(item => item.Checked ? (DateTimeOffset?)item.UpdatedAt : null)
            .ToListAsync(cancellationToken);

        return entities.Select(item => item.ToDomain()).ToArray();
    }

    public async Task<ShoppingListItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShoppingListItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == itemId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<decimal> GetNextSortOrderAsync(Guid shoppingListId, CancellationToken cancellationToken)
    {
        var currentMax = await dbContext.ShoppingListItems
            .Where(item => item.ShoppingListId == shoppingListId && !item.Checked)
            .MaxAsync(item => (decimal?)item.SortOrder, cancellationToken);

        return (currentMax ?? 0) + 1;
    }

    public Task<bool> SpaceBelongsToHouseholdAsync(Guid spaceId, Guid householdId, CancellationToken cancellationToken)
    {
        return dbContext.Spaces
            .AnyAsync(space => space.Id == spaceId && space.HouseholdId == householdId, cancellationToken);
    }

    public Task<bool> ItemBelongsToHouseholdAsync(Guid itemId, Guid householdId, CancellationToken cancellationToken)
    {
        return dbContext.Items
            .Join(
                dbContext.Spaces,
                item => item.SpaceId,
                space => space.Id,
                (item, space) => new { item.Id, space.HouseholdId })
            .AnyAsync(item => item.Id == itemId && item.HouseholdId == householdId, cancellationToken);
    }

    public async Task AddAsync(ShoppingListItem item, CancellationToken cancellationToken)
    {
        await dbContext.ShoppingListItems.AddAsync(ShoppingListItemEntity.FromDomain(item), cancellationToken);
    }

    public async Task UpdateAsync(ShoppingListItem item, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShoppingListItems
            .SingleOrDefaultAsync(existingItem => existingItem.Id == item.Id, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException($"Shopping list item '{item.Id}' was not found.");

        entity.UpdateFromDomain(item);
    }

    public async Task DeleteAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShoppingListItems
            .SingleOrDefaultAsync(item => item.Id == itemId, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException($"Shopping list item '{itemId}' was not found.");

        dbContext.ShoppingListItems.Remove(entity);
    }

    public async Task<int> DeleteCheckedAsync(Guid shoppingListId, CancellationToken cancellationToken)
    {
        var entities = await dbContext.ShoppingListItems
            .Where(item => item.ShoppingListId == shoppingListId && item.Checked)
            .ToListAsync(cancellationToken);

        dbContext.ShoppingListItems.RemoveRange(entities);
        return entities.Count;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
