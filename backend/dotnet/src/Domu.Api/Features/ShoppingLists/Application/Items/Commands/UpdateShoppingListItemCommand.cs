using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record UpdateShoppingListItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid ShoppingListId,
    Guid ItemId,
    string? Name,
    decimal? Quantity,
    decimal? ContainerQuantity,
    string? ContainerUnit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemIdLink,
    decimal? SortOrder);