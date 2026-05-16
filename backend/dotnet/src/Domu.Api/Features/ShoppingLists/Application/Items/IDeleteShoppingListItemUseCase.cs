namespace Domu.Api.Features.ShoppingLists.Application.Items;

public interface IDeleteShoppingListItemUseCase
{
    Task ExecuteAsync(ShoppingListItemCommand command, CancellationToken cancellationToken);
}
