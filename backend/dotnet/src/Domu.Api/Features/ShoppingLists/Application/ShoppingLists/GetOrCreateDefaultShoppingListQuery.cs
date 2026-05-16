namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed record GetOrCreateDefaultShoppingListQuery(Guid UserId, Guid HouseholdId);
