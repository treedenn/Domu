namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record DeleteShoppingListCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
