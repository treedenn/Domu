using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class ClearCheckedShoppingListItemsUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService)
    : IClearCheckedShoppingListItemsUseCase
{
    public async Task ExecuteAsync(ClearCheckedShoppingListItemsCommand command, CancellationToken cancellationToken)
    {
        await ShoppingListItemUseCaseGuards.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.UserId,
            cancellationToken);

        await shoppingListItemRepository.DeleteCheckedAsync(command.ShoppingListId, cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}
