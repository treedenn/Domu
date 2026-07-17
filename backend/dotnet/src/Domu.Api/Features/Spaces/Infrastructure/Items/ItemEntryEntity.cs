using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Infrastructure.Items;

public sealed class ItemEntryEntity
{
    private ItemEntryEntity()
    {
    }

    public ItemEntryEntity(
        Guid id,
        Guid itemId,
        decimal initialQuantity,
        decimal currentQuantity,
        ItemUnit unit,
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
        if (initialQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Item entry initial quantity must be >= 0.");
        if (currentQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(currentQuantity), "Item entry current quantity must be >= 0.");
        if (currentQuantity > initialQuantity)
            throw new ArgumentException("Item entry current quantity cannot be greater than initial quantity.");
        if (!Enum.IsDefined(unit))
            throw new ArgumentException("Item unit is invalid.", nameof(unit));
        if (acquisitionDate is not null && expirationDate is not null && acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        InitialQuantity = initialQuantity;
        CurrentQuantity = currentQuantity;
        Unit = unit;
        State = state;
        AcquisitionDate = acquisitionDate;
        ExpirationDate = expirationDate;
    }

    public Guid Id { get; }
    public Guid ItemId { get; private set; }
    public decimal InitialQuantity { get; private set; }
    public decimal CurrentQuantity { get; private set; }
    public ItemUnit Unit { get; private set; } = ItemUnit.Piece;
    public ConsumableState State { get; private set; }
    public DateTimeOffset? AcquisitionDate { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }

    public ItemEntry ToDomain()
    {
        var entry = new ItemEntry(Id, ItemId);
        entry.SetDates(AcquisitionDate, ExpirationDate);
        entry.SetQuantities(InitialQuantity, CurrentQuantity);
        entry.SetUnit(Unit);
        entry.ChangeState(State);
        return entry;
    }

    public static ItemEntryEntity FromDomain(ItemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new ItemEntryEntity(
            entry.Id,
            entry.ItemId,
            entry.InitialQuantity,
            entry.CurrentQuantity,
            entry.Unit,
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
        InitialQuantity = entry.InitialQuantity;
        CurrentQuantity = entry.CurrentQuantity;
        Unit = entry.Unit;
        State = entry.State;
        AcquisitionDate = entry.AcquisitionDate;
        ExpirationDate = entry.ExpirationDate;
    }
}
