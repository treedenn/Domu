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

    public int Quantity { get; private set; }

    public ConsumableState State { get; private set; } = ConsumableState.Unknown;

    public void SetDates(DateTimeOffset? acquisitionDate, DateTimeOffset? expirationDate)
    {
        if (acquisitionDate is not null && expirationDate is not null && acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        _acquisitionDate = acquisitionDate;
        _expirationDate = expirationDate;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Item entry quantity must be >= 0.", nameof(quantity));

        Quantity = quantity;
    }

    public void ChangeState(ConsumableState state)
    {
        State = state;
    }
}
