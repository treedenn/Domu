using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemView(
    Guid Id,
    Guid SpaceId,
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
            item.SpaceId,
            item.Name,
            item.Category,
            item.Barcode,
            item.TotalQuantity,
            item.Entries
                .Select(ItemEntryView.FromDomain)
                .ToArray());
    }
}
