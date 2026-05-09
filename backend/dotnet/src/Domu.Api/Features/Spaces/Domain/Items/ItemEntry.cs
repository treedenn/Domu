namespace Domu.Api.Features.Spaces.Domain.Items;

// ItemEntry captures one concrete stock state for an item, such as expiration and opened/closed status.
public sealed class ItemEntry
{
    private DateTimeOffset? _acquisitionDate;
    private DateTimeOffset? _expirationDate;

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

    public DateTimeOffset? AcquisitionDate => _acquisitionDate;

    public DateTimeOffset? ExpirationDate => _expirationDate;

    public decimal InitialQuantity { get; private set; }

    public decimal CurrentQuantity { get; private set; }

    public ItemUnit Unit { get; private set; } = ItemUnit.Piece;

    public ItemContainerType ContainerType { get; private set; } = ItemContainerType.Unspecified;

    public ConsumableState State { get; private set; } = ConsumableState.Unspecified;

    public void SetDates(DateTimeOffset? acquisitionDate, DateTimeOffset? expirationDate)
    {
        if (acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        _acquisitionDate = acquisitionDate;
        _expirationDate = expirationDate;
    }

    public void SetQuantities(decimal initialQuantity, decimal currentQuantity)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Item entry initial quantity must be >= 0.", nameof(initialQuantity));
        if (currentQuantity < 0)
            throw new ArgumentException("Item entry current quantity must be >= 0.", nameof(currentQuantity));
        if (currentQuantity > initialQuantity)
            throw new ArgumentException("Item entry current quantity cannot be greater than initial quantity.");

        InitialQuantity = initialQuantity;
        CurrentQuantity = currentQuantity;
    }

    public void SetUnit(ItemUnit unit)
    {
        if (!Enum.IsDefined(unit))
            throw new ArgumentException("Item unit is invalid.", nameof(unit));

        Unit = unit;
    }

    public void SetContainerType(ItemContainerType containerType)
    {
        if (!Enum.IsDefined(containerType))
            throw new ArgumentException("Item container type is invalid.", nameof(containerType));

        ContainerType = containerType;
    }

    public void ChangeState(ConsumableState state)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentException("Consumable state is invalid.", nameof(state));

        State = state;
    }
}
