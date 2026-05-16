using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;

public sealed record ShoppingListView(
    Guid Id,
    Guid HouseholdId,
    string Name,
    bool IsDefault,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ArchivedAt)
{
    public static ShoppingListView FromDomain(ShoppingList shoppingList)
    {
        ArgumentNullException.ThrowIfNull(shoppingList);

        return new ShoppingListView(
            shoppingList.Id,
            shoppingList.HouseholdId,
            shoppingList.Name,
            shoppingList.IsDefault,
            shoppingList.CreatedByUserId,
            shoppingList.CreatedAt,
            shoppingList.UpdatedAt,
            shoppingList.ArchivedAt);
    }
}
