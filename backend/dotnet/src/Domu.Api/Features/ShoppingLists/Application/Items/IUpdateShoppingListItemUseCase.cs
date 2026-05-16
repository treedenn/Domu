using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface IUpdateShoppingListItemUseCase
{
    Task<ShoppingListItemView> ExecuteAsync(UpdateShoppingListItemCommand command, CancellationToken cancellationToken);
}
