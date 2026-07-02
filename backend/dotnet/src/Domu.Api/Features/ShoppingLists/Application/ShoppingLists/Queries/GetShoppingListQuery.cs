namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;

public sealed record GetShoppingListQuery(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
