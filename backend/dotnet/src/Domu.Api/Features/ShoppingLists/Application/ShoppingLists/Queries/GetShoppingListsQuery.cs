using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;

public sealed record GetShoppingListsQuery(DomuActor Actor, Guid HouseholdId);