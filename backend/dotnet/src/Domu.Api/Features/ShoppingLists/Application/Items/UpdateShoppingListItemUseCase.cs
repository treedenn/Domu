using Domu.Api.Features.Events.Application;
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
    IHouseholdEventRecorder? userEventRecorder = null)
{
    private readonly IHouseholdEventRecorder _userEventRecorder = userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        UpdateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.Actor,
            cancellationToken);

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
        item.ChangeQuantity(command.Quantity, now);
        item.ChangeContainer(command.ContainerQuantity, command.ContainerUnit, now);
        item.ChangeNote(command.Note, now);
        item.LinkSpace(command.SpaceId, now);
        item.LinkItem(command.ItemIdLink, now);
        if (command.SortOrder.HasValue)
            item.MoveTo(command.SortOrder.Value, now);

        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.ShoppingListItemUpdated,
            HouseholdEventTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(
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
