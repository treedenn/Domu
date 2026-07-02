namespace Domu.Api.Features.ShoppingLists.Application.Items.Queries;

public sealed record GetShoppingListItemsQuery(Guid UserId, Guid HouseholdId, Guid ShoppingListId);
