using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

internal static class ShoppingListItemUseCaseGuards
{
    public static async Task EnsureCanAccessListAsync(
        IShoppingListRepository shoppingListRepository,
        IHouseholdAccessService householdAccessService,
        Guid householdId,
        Guid shoppingListId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(householdId, userId, cancellationToken);

        var shoppingList = await shoppingListRepository.GetByIdAsync(shoppingListId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Shopping list '{shoppingListId}' was not found.");

        if (shoppingList.HouseholdId != householdId || shoppingList.ArchivedAt is not null)
            throw new KeyNotFoundException($"Shopping list '{shoppingListId}' was not found.");
    }

    public static async Task<ShoppingListItem> GetAccessibleItemAsync(
        IShoppingListRepository shoppingListRepository,
        IShoppingListItemRepository shoppingListItemRepository,
        IHouseholdAccessService householdAccessService,
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            householdId,
            shoppingListId,
            userId,
            cancellationToken);

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
            && !await shoppingListItemRepository.SpaceBelongsToHouseholdAsync(spaceId.Value, householdId, cancellationToken))
            throw new KeyNotFoundException($"Space '{spaceId}' was not found.");

        if (itemId is not null
            && !await shoppingListItemRepository.ItemBelongsToHouseholdAsync(itemId.Value, householdId, cancellationToken))
            throw new KeyNotFoundException($"Item '{itemId}' was not found.");
    }
}
