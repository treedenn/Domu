using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class SetShoppingListItemCheckedStateUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        SetShoppingListItemCheckedStateCommand command,
        CancellationToken cancellationToken)
    {
        var item = await ShoppingListPermissionPolicy.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, command.ItemId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var activityAction = command.IsChecked
            ? HouseholdActivityActions.ShoppingListItemChecked
            : HouseholdActivityActions.ShoppingListItemUnchecked;

        if (command.IsChecked)
        {
            var memberId = await householdAccessService.GetRequiredMemberIdAsync(
                command.Actor, command.HouseholdId, cancellationToken);
            item.Check(memberId, now);
        }
        else
        {
            item.Uncheck(now);
        }

        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            activityAction,
            HouseholdActivityTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            ActivityMetadata.From(("shoppingListId", command.ShoppingListId)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}