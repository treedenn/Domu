using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface IGetShoppingListItemsUseCase
{
    Task<IReadOnlyList<ShoppingListItemView>> ExecuteAsync(
        GetShoppingListItemsQuery query,
        CancellationToken cancellationToken);
}
