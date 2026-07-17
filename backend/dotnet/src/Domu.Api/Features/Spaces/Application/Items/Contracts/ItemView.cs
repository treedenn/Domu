using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemView(
    Guid Id,
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    int TotalCount,
    IReadOnlyList<ItemEntryView> Entries,
    int? DefaultPurchaseCount = null,
    decimal? DefaultPurchaseAmountPerUnit = null,
    ItemUnit? DefaultPurchaseUnit = null)
{
    public static ItemView FromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemView(
            item.Id,
            item.SpaceId,
            item.Name,
            item.Category,
            item.Barcode,
            item.TotalCount,
            item.Entries
                .Select(ItemEntryView.FromDomain)
                .ToArray(),
            item.DefaultPurchaseCount,
            item.DefaultPurchaseAmountPerUnit,
            item.DefaultPurchaseUnit);
    }
}
