using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Application.Items.Contracts;

public sealed record ItemView(
    Guid Id,
    Guid LocationId,
    string Name,
    string? Category,
    string? Barcode,
    int TotalQuantity,
    IReadOnlyList<ItemEntryView> Entries)
{
    public static ItemView FromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemView(
            item.Id,
            item.LocationId,
            item.Name,
            item.Category,
            item.Barcode,
            item.TotalQuantity,
            item.Entries
                .Select(ItemEntryView.FromDomain)
                .ToArray());
    }
}
