namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed record ShoppingListItemCommand(Guid UserId, Guid HouseholdId, Guid ShoppingListId, Guid ItemId);
