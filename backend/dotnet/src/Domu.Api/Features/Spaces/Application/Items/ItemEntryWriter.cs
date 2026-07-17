using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

internal static class ItemEntryWriter
{
    public static void ReplaceEntries(Item item, IReadOnlyCollection<ItemEntryDraft>? entries)
    {
        ArgumentNullException.ThrowIfNull(item);

        var requestedEntries = entries ?? [];
        var requestedIds = requestedEntries
            .Where(entry => entry.Id.HasValue)
            .Select(entry => entry.Id!.Value)
            .ToHashSet();

        foreach (var existingEntry in item.Entries.ToArray())
            if (!requestedIds.Contains(existingEntry.Id))
                item.RemoveEntry(existingEntry.Id);

        foreach (var entryInput in requestedEntries)
        {
            var entryId = entryInput.Id ?? Guid.CreateVersion7();
            var entry = item.Entries.SingleOrDefault(existingEntry => existingEntry.Id == entryId);

            if (entry is null)
            {
                entry = new ItemEntry(entryId, item.Id);
                item.AddEntry(entry);
            }

            entry.SetDates(entryInput.AcquisitionDate, entryInput.ExpirationDate);
            entry.SetQuantities(entryInput.OriginalQuantity, entryInput.CurrentQuantity);
            entry.SetUnit(entryInput.Unit);
            entry.ChangeState(entryInput.State);
        }
    }
}
