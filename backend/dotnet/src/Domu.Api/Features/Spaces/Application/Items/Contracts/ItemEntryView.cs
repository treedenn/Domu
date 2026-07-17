using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemEntryView(
    Guid Id,
    int Count,
    decimal? OriginalAmountPerUnit,
    decimal? CurrentAmountPerUnit,
    ItemUnit Unit,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate)
{
    public static ItemEntryView FromDomain(ItemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new ItemEntryView(
            entry.Id,
            entry.Count,
            entry.OriginalAmountPerUnit,
            entry.CurrentAmountPerUnit,
            entry.Unit,
            entry.State,
            entry.AcquisitionDate,
            entry.ExpirationDate);
    }
}
