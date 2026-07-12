using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record CreateShoppingListItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid ShoppingListId,
    string Name,
    decimal? Quantity,
    decimal? ContainerQuantity,
    string? ContainerUnit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId);
