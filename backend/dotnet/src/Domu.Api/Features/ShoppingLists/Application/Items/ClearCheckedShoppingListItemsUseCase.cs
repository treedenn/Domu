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
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task ExecuteAsync(ClearCheckedShoppingListItemsCommand command, CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.UserId,
            cancellationToken);

        await shoppingListItemRepository.DeleteCheckedAsync(command.ShoppingListId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId,
            UserEventActions.ShoppingListCheckedItemsCleared,
            UserEventTargetTypes.ShoppingList,
            command.ShoppingListId,
            command.HouseholdId,
            EventMetadata.Empty(),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
    }
}
