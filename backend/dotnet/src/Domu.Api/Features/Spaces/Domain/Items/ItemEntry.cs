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

    public int Count { get; private set; }
    public decimal? OriginalAmountPerUnit { get; private set; }
    public decimal? CurrentAmountPerUnit { get; private set; }

    public ItemUnit Unit { get; private set; } = ItemUnit.Unspecified;

    public ConsumableState State { get; private set; } = ConsumableState.Unspecified;

    public void SetDates(DateTimeOffset? acquisitionDate, DateTimeOffset? expirationDate)
    {
        if (acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        AcquisitionDate = acquisitionDate?.ToUniversalTime();
        ExpirationDate = expirationDate?.ToUniversalTime();
    }

    public void SetBatch(int count, decimal? originalAmountPerUnit, decimal? currentAmountPerUnit)
    {
        if (count <= 0)
            throw new ArgumentException("Item entry count must be greater than 0.", nameof(count));
        if (originalAmountPerUnit.HasValue != currentAmountPerUnit.HasValue)
            throw new ArgumentException("Item entry original and current amounts must be supplied together.");
        if (!originalAmountPerUnit.HasValue && Unit != ItemUnit.Unspecified)
            throw new ArgumentException("Item entry without amounts must use the unspecified unit.");
        if (originalAmountPerUnit < 0)
            throw new ArgumentException("Item entry original amount per unit must be >= 0.", nameof(originalAmountPerUnit));
        if (currentAmountPerUnit < 0 || currentAmountPerUnit > originalAmountPerUnit)
            throw new ArgumentException("Item entry current amount per unit must be between 0 and the original amount.", nameof(currentAmountPerUnit));

        Count = count;
        OriginalAmountPerUnit = originalAmountPerUnit;
        CurrentAmountPerUnit = currentAmountPerUnit;
    }

    public void SetUnit(ItemUnit unit)
    {
        if (!Enum.IsDefined(unit))
            throw new ArgumentException("Item unit is invalid.", nameof(unit));

        if (!CurrentAmountPerUnit.HasValue && unit != ItemUnit.Unspecified)
            throw new ArgumentException("Item entry without amounts must use the unspecified unit.", nameof(unit));
        Unit = unit;
    }

    public void ChangeState(ConsumableState state)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentException("Consumable state is invalid.", nameof(state));

        State = state;
    }
}
