using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Interface.Items;

public sealed record UpdateShoppingListItemRequest(
    string? Name,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId,
    decimal? SortOrder,
    int Count = 1,
    decimal? PlannedAmountPerUnit = null,
    ItemUnit? PlannedUnit = null);
