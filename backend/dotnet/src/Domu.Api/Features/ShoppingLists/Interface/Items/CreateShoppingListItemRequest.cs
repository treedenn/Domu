using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Interface.Items;

public sealed record CreateShoppingListItemRequest(
    string Name,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId,
    int Count = 1,
    decimal? AmountPerUnit = null,
    ItemUnit? Unit = null);
