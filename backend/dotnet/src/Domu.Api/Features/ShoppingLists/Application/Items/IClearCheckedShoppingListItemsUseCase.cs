namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface IClearCheckedShoppingListItemsUseCase
{
    Task ExecuteAsync(ClearCheckedShoppingListItemsCommand command, CancellationToken cancellationToken);
}
