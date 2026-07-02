namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;

public sealed record GetShoppingListsQuery(Guid UserId, Guid HouseholdId);
