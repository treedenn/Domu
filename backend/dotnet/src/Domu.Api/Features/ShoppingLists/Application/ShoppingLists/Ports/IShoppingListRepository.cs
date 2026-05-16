using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetActiveDefaultByHouseholdAsync(Guid householdId, CancellationToken cancellationToken);
    Task<ShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken);
    Task AddAsync(ShoppingList shoppingList, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
