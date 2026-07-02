namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record UpdateShoppingListItemCommand(
    Guid UserId,
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
