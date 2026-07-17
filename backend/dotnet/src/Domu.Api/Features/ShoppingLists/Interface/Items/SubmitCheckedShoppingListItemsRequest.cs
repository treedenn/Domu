using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Interface.Items;

public sealed record SubmitCheckedShoppingListItemsRequest(IReadOnlyCollection<SubmitCheckedShoppingListItemRequest> Items);
public sealed record SubmitCheckedShoppingListItemRequest(Guid ShoppingListItemId, decimal? AmountPerUnit, ItemUnit? Unit, ConsumableState? State, DateTimeOffset? AcquisitionDate, DateTimeOffset? ExpirationDate);
