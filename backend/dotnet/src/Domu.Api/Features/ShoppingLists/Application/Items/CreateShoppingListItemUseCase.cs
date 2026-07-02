using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class CreateShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        CreateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.UserId,
            cancellationToken);

        await ShoppingListPermissionPolicy.ValidateReferencesAsync(
            shoppingListItemRepository,
            command.HouseholdId,
            command.SpaceId,
            command.ItemId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var item = new ShoppingListItem(
            Guid.CreateVersion7(),
            command.HouseholdId,
            command.ShoppingListId,
            command.Name,
            command.UserId,
            now,
            now,
            await shoppingListItemRepository.GetNextSortOrderAsync(command.ShoppingListId, cancellationToken));

        item.ChangeQuantity(command.Quantity, now);
        item.ChangeContainer(command.ContainerQuantity, command.ContainerUnit, now);
        item.ChangeNote(command.Note, now);
        item.LinkSpace(command.SpaceId, now);
        item.LinkItem(command.ItemId, now);

        await shoppingListItemRepository.AddAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId,
            UserEventActions.ShoppingListItemCreated,
            UserEventTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(
                ("shoppingListId", command.ShoppingListId),
                ("name", item.Name),
                ("spaceId", item.SpaceId),
                ("itemId", item.ItemId)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
