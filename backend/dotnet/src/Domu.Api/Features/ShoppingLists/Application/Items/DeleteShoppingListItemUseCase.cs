using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class DeleteShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IHouseholdEventRecorder? userEventRecorder = null)
{
    private readonly IHouseholdEventRecorder _userEventRecorder = userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task ExecuteAsync(DeleteShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        var item = await ShoppingListPermissionPolicy.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.ItemId,
            command.Actor,
            cancellationToken);

        await shoppingListItemRepository.DeleteAsync(command.ItemId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.ShoppingListItemDeleted,
            HouseholdEventTargetTypes.ShoppingListItem,
            command.ItemId,
            command.HouseholdId,
            EventMetadata.From(("shoppingListId", command.ShoppingListId), ("name", item.Name)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}
