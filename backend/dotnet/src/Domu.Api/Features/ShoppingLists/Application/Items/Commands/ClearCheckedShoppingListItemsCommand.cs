using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record ClearCheckedShoppingListItemsCommand(DomuActor Actor, Guid HouseholdId, Guid ShoppingListId);
