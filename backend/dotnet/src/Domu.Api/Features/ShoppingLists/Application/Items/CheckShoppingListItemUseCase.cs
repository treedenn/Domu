using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class CheckShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IUserEventRecorder? userEventRecorder = null)
    : ICheckShoppingListItemUseCase
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

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

        item.Check(command.UserId, DateTimeOffset.UtcNow);
        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId,
            UserEventActions.ShoppingListItemChecked,
            UserEventTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(("shoppingListId", command.ShoppingListId)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
