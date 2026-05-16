using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface ICheckShoppingListItemUseCase
{
    Task<ShoppingListItemView> ExecuteAsync(ShoppingListItemCommand command, CancellationToken cancellationToken);
}
