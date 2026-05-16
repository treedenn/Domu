namespace Domu.Api.Features.ShoppingLists.Interface.Items;

public sealed record CreateShoppingListItemRequest(
    string Name,
    decimal? Quantity,
    decimal? ContainerQuantity,
    string? ContainerUnit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId);
