using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

public interface IInventoryItemLookup
{
    Task<InventoryItemPurchaseDefaults?> GetAsync(Guid householdId, Guid itemId, CancellationToken cancellationToken);
}

public sealed record InventoryItemPurchaseDefaults(string Name, int? Count, decimal? AmountPerUnit, ItemUnit? Unit);
