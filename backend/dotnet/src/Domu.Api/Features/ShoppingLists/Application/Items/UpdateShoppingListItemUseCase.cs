using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class UpdateShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        UpdateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, cancellationToken);

        await ShoppingListPermissionPolicy.ValidateReferencesAsync(
            shoppingListItemRepository,
            command.HouseholdId,
            command.SpaceId,
            command.ItemIdLink,
            cancellationToken);

        var item = await ShoppingListPermissionPolicy.GetListItemAsync(
            shoppingListItemRepository,
            command.ShoppingListId,
            command.ItemId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (command.Name is not null)
            item.Rename(command.Name, now);
        item.ChangeNote(command.Note, now);
        item.LinkSpace(command.SpaceId, now);
        item.LinkItem(command.ItemIdLink, now);
        item.SetPurchaseDetails(command.Count, command.AmountPerUnit, command.Unit, now);
        if (command.SortOrder.HasValue)
            item.MoveTo(command.SortOrder.Value, now);

        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.ShoppingListItemUpdated,
            HouseholdActivityTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            ActivityMetadata.From(
                ("shoppingListId", command.ShoppingListId),
                ("name", item.Name),
                ("spaceId", item.SpaceId),
                ("itemId", item.ItemId),
                ("sortOrder", item.SortOrder)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
