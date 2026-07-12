using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class ClearCheckedShoppingListItemsUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task ExecuteAsync(ClearCheckedShoppingListItemsCommand command, CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, cancellationToken);

        await shoppingListItemRepository.DeleteCheckedAsync(command.ShoppingListId, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.ShoppingListCheckedItemsCleared,
            HouseholdActivityTargetTypes.ShoppingList,
            command.ShoppingListId,
            command.HouseholdId,
            ActivityMetadata.Empty(),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}