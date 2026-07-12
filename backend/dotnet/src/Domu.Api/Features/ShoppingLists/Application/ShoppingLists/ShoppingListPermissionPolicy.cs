using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

internal static class ShoppingListPermissionPolicy
{
    public static async Task EnsureCanAccessListAsync(IShoppingListRepository shoppingListRepository,
        IHouseholdAccessService householdAccessService,
        DomuActor actor,
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        await GetAccessibleListAsync(
            shoppingListRepository, householdAccessService, householdId, shoppingListId, actor, cancellationToken);
    }

    public static async Task<ShoppingList> GetAccessibleListAsync(
        IShoppingListRepository shoppingListRepository,
        IHouseholdAccessService householdAccessService,
        Guid householdId,
        Guid shoppingListId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(actor, householdId, cancellationToken);

        var shoppingList = await shoppingListRepository.GetByIdAsync(shoppingListId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Shopping list '{shoppingListId}' was not found.");

        if (shoppingList.HouseholdId != householdId || shoppingList.ArchivedAt is not null)
            throw new KeyNotFoundException($"Shopping list '{shoppingListId}' was not found.");

        return shoppingList;
    }

    public static async Task<ShoppingListItem> GetAccessibleItemAsync(
        IShoppingListRepository shoppingListRepository,
        IShoppingListItemRepository shoppingListItemRepository,
        IHouseholdAccessService householdAccessService,
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        await EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            actor,
            householdId, shoppingListId, cancellationToken);

        return await GetListItemAsync(shoppingListItemRepository, shoppingListId, itemId, cancellationToken);
    }

    public static async Task<ShoppingListItem> GetListItemAsync(
        IShoppingListItemRepository shoppingListItemRepository,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await shoppingListItemRepository.GetItemByIdAsync(itemId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Shopping list item '{itemId}' was not found.");

        if (item.ShoppingListId != shoppingListId)
            throw new KeyNotFoundException($"Shopping list item '{itemId}' was not found.");

        return item;
    }

    public static async Task ValidateReferencesAsync(
        IShoppingListItemRepository shoppingListItemRepository,
        Guid householdId,
        Guid? spaceId,
        Guid? itemId,
        CancellationToken cancellationToken)
    {
        if (spaceId is not null
            && !await shoppingListItemRepository.SpaceBelongsToHouseholdAsync(spaceId.Value, householdId,
                cancellationToken))
            throw new KeyNotFoundException($"Space '{spaceId}' was not found.");

        if (itemId is not null
            && !await shoppingListItemRepository.ItemBelongsToHouseholdAsync(itemId.Value, householdId,
                cancellationToken))
            throw new KeyNotFoundException($"Item '{itemId}' was not found.");
    }
}