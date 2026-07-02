namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record UpdateShoppingListCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId, string Name);
