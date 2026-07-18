using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Features.Spaces.Application.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class CreateShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IInventoryItemLookup? inventoryItemLookup = null,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<ShoppingListItemView> ExecuteAsync(
        CreateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.Actor,
            command.HouseholdId, command.ShoppingListId, cancellationToken);
        var memberId = await householdAccessService.GetRequiredMemberIdAsync(
            command.Actor, command.HouseholdId, cancellationToken);

        await ShoppingListPermissionPolicy.ValidateReferencesAsync(
            shoppingListItemRepository,
            command.HouseholdId,
            command.SpaceId,
            command.ItemId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var source = command.ItemId is null || inventoryItemLookup is null
            ? null
            : await inventoryItemLookup.GetAsync(command.HouseholdId, command.ItemId.Value, cancellationToken);
        if (command.ItemId is not null && inventoryItemLookup is not null && source is null)
            throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");
        var item = new ShoppingListItem(
            Guid.CreateVersion7(),
            command.HouseholdId,
            command.ShoppingListId,
            source?.Name ?? command.Name,
            memberId,
            now,
            now,
            await shoppingListItemRepository.GetNextSortOrderAsync(command.ShoppingListId, cancellationToken));

        item.ChangeNote(command.Note, now);
        item.LinkSpace(command.SpaceId, now);
        item.LinkItem(command.ItemId, now);
        item.SetPurchaseDetails(source?.Count ?? command.Count, source?.AmountPerUnit ?? command.AmountPerUnit, source?.Unit ?? command.Unit, now);

        await shoppingListItemRepository.AddAsync(item, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.ShoppingListItemCreated,
            HouseholdActivityTargetTypes.ShoppingListItem,
            item.Id,
            command.HouseholdId,
            ActivityMetadata.From(
                ("shoppingListId", command.ShoppingListId),
                ("name", item.Name),
                ("spaceId", item.SpaceId),
                ("itemId", item.ItemId)),
            cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
