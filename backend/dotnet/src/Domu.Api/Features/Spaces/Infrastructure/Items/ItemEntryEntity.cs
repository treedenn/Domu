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
        int count,
        decimal? originalAmountPerUnit,
        decimal? currentAmountPerUnit,
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
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (originalAmountPerUnit.HasValue != currentAmountPerUnit.HasValue || originalAmountPerUnit < 0 || currentAmountPerUnit < 0 || currentAmountPerUnit > originalAmountPerUnit) throw new ArgumentException("Item entry amounts are invalid.");
        if (!Enum.IsDefined(unit))
            throw new ArgumentException("Item unit is invalid.", nameof(unit));
        if (acquisitionDate is not null && expirationDate is not null && acquisitionDate > expirationDate)
            throw new ArgumentException("Item entry acquisition date cannot be after expiration date.");

        Unit = unit;
        State = state;
        AcquisitionDate = acquisitionDate;
        ExpirationDate = expirationDate;
        Count = count;
        OriginalAmountPerUnit = originalAmountPerUnit;
        CurrentAmountPerUnit = currentAmountPerUnit;
    }

    public Guid Id { get; }
    public Guid ItemId { get; private set; }
    public int Count { get; private set; }
    public decimal? OriginalAmountPerUnit { get; private set; }
    public decimal? CurrentAmountPerUnit { get; private set; }
    public ItemUnit Unit { get; private set; } = ItemUnit.Unspecified;
    public ConsumableState State { get; private set; }
    public DateTimeOffset? AcquisitionDate { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }

    public ItemEntry ToDomain()
    {
        var entry = new ItemEntry(Id, ItemId);
        entry.SetDates(AcquisitionDate, ExpirationDate);
        entry.SetBatch(Count, OriginalAmountPerUnit, CurrentAmountPerUnit);
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
            entry.Count,
            entry.OriginalAmountPerUnit,
            entry.CurrentAmountPerUnit,
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
        Count = entry.Count;
        OriginalAmountPerUnit = entry.OriginalAmountPerUnit;
        CurrentAmountPerUnit = entry.CurrentAmountPerUnit;
        Unit = entry.Unit;
        State = entry.State;
        AcquisitionDate = entry.AcquisitionDate;
        ExpirationDate = entry.ExpirationDate;
    }
}
