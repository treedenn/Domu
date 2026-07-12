using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;

public sealed record UpdateShoppingListCommand(DomuActor Actor, Guid HouseholdId, Guid ShoppingListId, string Name, bool Archived);
