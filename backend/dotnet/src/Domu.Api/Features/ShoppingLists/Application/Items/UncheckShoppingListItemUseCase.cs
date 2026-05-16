using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class UncheckShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService)
    : IUncheckShoppingListItemUseCase
{
    public async Task<ShoppingListItemView> ExecuteAsync(
        ShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await ShoppingListItemUseCaseGuards.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.ItemId,
            command.UserId,
            cancellationToken);

        item.Uncheck(DateTimeOffset.UtcNow);
        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
