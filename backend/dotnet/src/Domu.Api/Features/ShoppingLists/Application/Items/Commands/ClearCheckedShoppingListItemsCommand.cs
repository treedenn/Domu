namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record ClearCheckedShoppingListItemsCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
