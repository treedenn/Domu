namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record CreateShoppingListCommand(Guid UserId, Guid HouseholdId, string Name);
