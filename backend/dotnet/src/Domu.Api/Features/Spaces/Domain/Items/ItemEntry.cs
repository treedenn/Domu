namespace Domu.Api.Features.Spaces.Domain.Items;

// ItemEntry captures one concrete stock state for an item, such as expiration and opened/closed status.
public sealed class ItemEntry
{
    public ItemEntry(Guid id, Guid itemId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Item entry id cannot be empty.")
            : id;
        ItemId = itemId == Guid.Empty
            ? throw new ArgumentException("Item id cannot be empty.")
            : itemId;
    }

    public Guid Id { get; }
    public Guid ItemId { get; }

    public DateTimeOffset? AcquisitionDate { get; private set; }

    public DateTimeOffset? ExpirationDate { get; private set; }

    public decimal OriginalQuantity { get; private set; }

    public decimal CurrentQuantity { get; private set; }

    public ItemUnit Unit { get; private set; } = ItemUnit.Piece;

    public ConsumableState State { get; private set; } = ConsumableState.Unspecified;

    public void SetDates(DateTimeOffset? acquisitionDate, DateTimeOffset? expirationDate)
    {
        if (acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        AcquisitionDate = acquisitionDate;
        ExpirationDate = expirationDate;
    }

    public void SetQuantities(decimal originalQuantity, decimal currentQuantity)
    {
        if (originalQuantity < 0)
            throw new ArgumentException("Item entry original quantity must be >= 0.", nameof(originalQuantity));
        if (currentQuantity < 0)
            throw new ArgumentException("Item entry current quantity must be >= 0.", nameof(currentQuantity));
        if (currentQuantity > originalQuantity)
            throw new ArgumentException("Item entry current quantity cannot be greater than original quantity.");

        OriginalQuantity = originalQuantity;
        CurrentQuantity = currentQuantity;
    }

    public void SetUnit(ItemUnit unit)
    {
        if (!Enum.IsDefined(unit))
            throw new ArgumentException("Item unit is invalid.", nameof(unit));

        Unit = unit;
    }

    public void ChangeState(ConsumableState state)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentException("Consumable state is invalid.", nameof(state));

        State = state;
    }
}
