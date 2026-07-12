using Domu.Api.Features.Activities.Application;
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
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task ExecuteAsync(DeleteShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        var item = await ShoppingListPermissionPolicy.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, command.ItemId, cancellationToken);

        await shoppingListItemRepository.DeleteAsync(command.ItemId, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.ShoppingListItemDeleted,
            HouseholdActivityTargetTypes.ShoppingListItem,
            command.ItemId,
            command.HouseholdId,
            ActivityMetadata.From(("shoppingListId", command.ShoppingListId), ("name", item.Name)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}