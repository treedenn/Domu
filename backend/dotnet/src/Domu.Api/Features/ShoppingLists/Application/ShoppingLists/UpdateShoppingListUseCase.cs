using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class UpdateShoppingListUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<ShoppingListView> ExecuteAsync(UpdateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var list = await ShoppingListPermissionPolicy.GetAccessibleListAsync(
            shoppingListRepository, householdAccessService, command.HouseholdId, command.ShoppingListId, command.UserId, cancellationToken);
        list.Rename(command.Name, DateTimeOffset.UtcNow);
        if (command.Archived)
        {
            list.Archive(DateTimeOffset.UtcNow);
        }
        else
        {
            list.Unarchive();
        }

        await shoppingListRepository.UpdateAsync(list, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId, UserEventActions.ShoppingListUpdated, UserEventTargetTypes.ShoppingList,
            list.Id, command.HouseholdId, EventMetadata.From(("name", list.Name)), cancellationToken);
        await shoppingListRepository.SaveChangesAsync(cancellationToken);
        return ShoppingListView.FromDomain(list);
    }
}
