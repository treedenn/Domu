using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Queries;

public sealed record GetShoppingListItemsQuery(DomuActor Actor, Guid HouseholdId, Guid ShoppingListId);