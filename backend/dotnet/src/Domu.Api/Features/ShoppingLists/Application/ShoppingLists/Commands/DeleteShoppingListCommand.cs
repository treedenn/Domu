using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record DeleteShoppingListCommand(DomuActor Actor, Guid HouseholdId, Guid ShoppingListId);
