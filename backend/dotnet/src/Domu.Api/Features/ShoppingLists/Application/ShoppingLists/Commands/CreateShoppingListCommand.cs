using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record CreateShoppingListCommand(DomuActor Actor, Guid HouseholdId, string Name);
