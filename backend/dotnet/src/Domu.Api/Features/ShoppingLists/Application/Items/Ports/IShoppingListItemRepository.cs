using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Ports;

public interface IShoppingListItemRepository
{
    Task<IReadOnlyList<ShoppingListItem>> GetItemsAsync(Guid shoppingListId, CancellationToken cancellationToken);
    Task<ShoppingListItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken);
    Task<decimal> GetNextSortOrderAsync(Guid shoppingListId, CancellationToken cancellationToken);
    Task<bool> SpaceBelongsToHouseholdAsync(Guid spaceId, Guid householdId, CancellationToken cancellationToken);
    Task<bool> ItemBelongsToHouseholdAsync(Guid itemId, Guid householdId, CancellationToken cancellationToken);
    Task AddAsync(ShoppingListItem item, CancellationToken cancellationToken);
    Task UpdateAsync(ShoppingListItem item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid itemId, CancellationToken cancellationToken);
    Task<int> DeleteCheckedAsync(Guid shoppingListId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
