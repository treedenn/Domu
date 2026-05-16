using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface ICreateShoppingListItemUseCase
{
    Task<ShoppingListItemView> ExecuteAsync(CreateShoppingListItemCommand command, CancellationToken cancellationToken);
}
