using Domu.Api.Features.Events.Application;
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
    IHouseholdEventRecorder? userEventRecorder = null)
{
    private readonly IHouseholdEventRecorder _userEventRecorder =
        userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task ExecuteAsync(ClearCheckedShoppingListItemsCommand command, CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, cancellationToken);

        await shoppingListItemRepository.DeleteCheckedAsync(command.ShoppingListId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.ShoppingListCheckedItemsCleared,
            HouseholdEventTargetTypes.ShoppingList,
            command.ShoppingListId,
            command.HouseholdId,
            EventMetadata.Empty(),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}