using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class DeleteShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService)
    : IDeleteShoppingListItemUseCase
{
    public async Task ExecuteAsync(ShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        _ = await ShoppingListItemUseCaseGuards.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.ItemId,
            command.UserId,
            cancellationToken);

        await shoppingListItemRepository.DeleteAsync(command.ItemId, cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}
