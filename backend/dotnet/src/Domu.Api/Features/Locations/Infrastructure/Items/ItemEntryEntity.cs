using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Infrastructure.Items;

public sealed class ItemEntryEntity
{
    private ItemEntryEntity()
    {
    }

    public ItemEntryEntity(
        Guid id,
        Guid itemId,
        int quantity,
        ConsumableState state,
        DateTimeOffset? acquisitionDate,
        DateTimeOffset? expirationDate)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Item entry id cannot be empty.", nameof(id))
            : id;
        ItemId = itemId == Guid.Empty
            ? throw new ArgumentException("Item id cannot be empty.", nameof(itemId))
            : itemId;
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Item entry quantity must be >= 0.");
        if (acquisitionDate is not null && expirationDate is not null && acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        Quantity = quantity;
        State = state;
        AcquisitionDate = acquisitionDate;
        ExpirationDate = expirationDate;
    }

    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public int Quantity { get; private set; }
    public ConsumableState State { get; private set; }
    public DateTimeOffset? AcquisitionDate { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }

    public ItemEntry ToDomain()
    {
        var entry = new ItemEntry(Id, ItemId);
        entry.SetDates(AcquisitionDate, ExpirationDate);
        entry.SetQuantity(Quantity);
        entry.ChangeState(State);
        return entry;
    }

    public static ItemEntryEntity FromDomain(ItemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new ItemEntryEntity(
            entry.Id,
            entry.ItemId,
            entry.Quantity,
            entry.State,
            entry.AcquisitionDate,
            entry.ExpirationDate);
    }

    public void UpdateFromDomain(ItemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.Id != Id)
            throw new ArgumentException("Cannot update item entry entity from a different entry.", nameof(entry));

        ItemId = entry.ItemId;
        Quantity = entry.Quantity;
        State = entry.State;
        AcquisitionDate = entry.AcquisitionDate;
        ExpirationDate = entry.ExpirationDate;
    }
}
