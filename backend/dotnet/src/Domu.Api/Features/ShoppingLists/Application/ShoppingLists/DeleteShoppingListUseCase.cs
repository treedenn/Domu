using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class DeleteShoppingListUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task ExecuteAsync(DeleteShoppingListCommand command, CancellationToken cancellationToken)
    {
        var list = await ShoppingListPermissionPolicy.GetAccessibleListAsync(
            shoppingListRepository, householdAccessService, command.HouseholdId, command.ShoppingListId, command.Actor,
            cancellationToken);
        list.Archive(DateTimeOffset.UtcNow);

        await shoppingListRepository.UpdateAsync(list, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor, HouseholdActivityActions.ShoppingListDeleted, HouseholdActivityTargetTypes.ShoppingList,
            list.Id, command.HouseholdId, ActivityMetadata.From(("name", list.Name)), cancellationToken);
        await shoppingListRepository.SaveChangesAsync(cancellationToken);
    }
}