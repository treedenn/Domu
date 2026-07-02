namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record DeleteShoppingListItemCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId, Guid ItemId);
