namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed record GetShoppingListItemsQuery(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
