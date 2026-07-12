using Domu.Api.Features.Events.Application;
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
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        SetShoppingListItemCheckedStateCommand command,
        CancellationToken cancellationToken)
    {
        var item = await ShoppingListPermissionPolicy.GetAccessibleItemAsync(
            shoppingListRepository,
            shoppingListItemRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.ItemId,
            command.UserId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var eventAction = command.IsChecked
            ? UserEventActions.ShoppingListItemChecked
            : UserEventActions.ShoppingListItemUnchecked;

        if (command.IsChecked)
        {
            var memberId = await householdAccessService.GetRequiredMemberIdAsync(
                command.HouseholdId, command.UserId, cancellationToken);
            item.Check(memberId, now);
        }
        else
        {
            item.Uncheck(now);
        }

        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId,
            eventAction,
            UserEventTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(("shoppingListId", command.ShoppingListId)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
