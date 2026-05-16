namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed record ClearCheckedShoppingListItemsCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
