using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class InventoryItemLookup(IItemRepository itemRepository, ISpaceAccessService spaceAccessService) : IInventoryItemLookup
{
    public async Task<InventoryItemPurchaseDefaults?> GetAsync(Guid householdId, Guid itemId, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item is null) return null;
        try { await spaceAccessService.EnsureSpaceBelongsToHouseholdAsync(item.SpaceId, householdId, cancellationToken); }
        catch (KeyNotFoundException) { return null; }
        return new(item.Name, item.DefaultPurchaseCount, item.DefaultPurchaseAmountPerUnit, item.DefaultPurchaseUnit);
    }
}
