namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed record CreateShoppingListItemCommand(
    Guid UserId,
    Guid HouseholdId,
    Guid ShoppingListId,
    string Name,
    decimal? Quantity,
    decimal? ContainerQuantity,
    string? ContainerUnit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId);
