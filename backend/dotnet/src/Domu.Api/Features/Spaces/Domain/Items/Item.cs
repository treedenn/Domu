namespace Domu.Api.Features.Spaces.Domain.Items;

// Item is a physical object that can be stored in a space.
public sealed class Item
{
    public const int NameMaxLength = 255;
    public const int CategoryMaxLength = 255;
    public const int BarcodeMaxLength = 128;

    private readonly HashSet<ItemEntry> _entries = [];

    public Item(Guid id, string name, Guid spaceId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Item id cannot be empty.", nameof(id))
            : id;
        MoveTo(spaceId);
        Rename(name);
    }

    public Guid Id { get; }
    public Guid SpaceId { get; private set; }
    public IReadOnlySet<ItemEntry> Entries => _entries.AsReadOnly();
    public int TotalCount => _entries.Sum(entry => entry.Count);

    public string Name { get; private set; } = null!;

    public string? Category { get; private set; }

    public string? Barcode { get; private set; }
    public int? DefaultPurchaseCount { get; private set; }
    public decimal? DefaultPurchaseAmountPerUnit { get; private set; }
    public ItemUnit? DefaultPurchaseUnit { get; private set; }

    public void SetDefaultPurchase(int? count, decimal? amountPerUnit, ItemUnit? unit)
    {
        if (count is null && amountPerUnit is null && unit is null)
        {
            DefaultPurchaseCount = null; DefaultPurchaseAmountPerUnit = null; DefaultPurchaseUnit = null; return;
        }
        if (count is null || count <= 0 || amountPerUnit.HasValue != unit.HasValue || amountPerUnit < 0 || (unit.HasValue && (!Enum.IsDefined(unit.Value) || unit == ItemUnit.Unspecified)))
            throw new ArgumentException("Item purchase defaults require a positive count; amount and unit must be supplied together.");
        DefaultPurchaseCount = count; DefaultPurchaseAmountPerUnit = amountPerUnit; DefaultPurchaseUnit = unit;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name cannot be empty.", nameof(name));
        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Item name cannot be longer than {NameMaxLength} characters.", nameof(name));

        Name = name;
    }

    public void MoveTo(Guid spaceId)
    {
        if (spaceId == Guid.Empty)
            throw new ArgumentException("Space id cannot be empty.", nameof(spaceId));

        SpaceId = spaceId;
    }

    public void ChangeCategory(string? category)
    {
        if (category is not null)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Item category cannot be empty.", nameof(category));
            if (category.Length > CategoryMaxLength)
                throw new ArgumentException(
                    $"Item category cannot be longer than {CategoryMaxLength} characters.",
                    nameof(category));
        }

        Category = category;
    }

    public void ChangeBarcode(string? barcode)
    {
        if (barcode is not null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Item barcode cannot be empty.", nameof(barcode));
            if (barcode.Length > BarcodeMaxLength)
                throw new ArgumentException(
                    $"Item barcode cannot be longer than {BarcodeMaxLength} characters.",
                    nameof(barcode));
        }

        Barcode = barcode;
    }

    public bool AddEntry(ItemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.ItemId != Id)
            throw new ArgumentException("Item entry must belong to this item.", nameof(entry));

        return _entries.Add(entry);
    }

    public bool RemoveEntry(Guid entryId)
    {
        var entry = _entries.FirstOrDefault(existingEntry => existingEntry.Id == entryId);
        if (entry is null)
            return false;

        return _entries.Remove(entry);
    }

    public ItemEntry SplitEntry(Guid entryId, int count, decimal? currentAmountPerUnit, ConsumableState state,
        DateTimeOffset? acquisitionDate = null, DateTimeOffset? expirationDate = null)
    {
        var source = _entries.SingleOrDefault(entry => entry.Id == entryId)
                     ?? throw new KeyNotFoundException($"Item entry '{entryId}' was not found.");
        if (count <= 0 || count >= source.Count)
            throw new ArgumentException("Split count must be positive and smaller than the source batch count.", nameof(count));

        if (source.CurrentAmountPerUnit.HasValue != currentAmountPerUnit.HasValue)
            throw new ArgumentException("Split amount detail must match the source entry.", nameof(currentAmountPerUnit));
        source.SetBatch(source.Count - count, source.OriginalAmountPerUnit, source.CurrentAmountPerUnit);
        var split = new ItemEntry(Guid.CreateVersion7(), Id);
        split.SetBatch(count, source.OriginalAmountPerUnit, currentAmountPerUnit);
        split.SetUnit(source.Unit);
        split.ChangeState(state);
        split.SetDates(acquisitionDate ?? source.AcquisitionDate, expirationDate ?? source.ExpirationDate);
        _entries.Add(split);
        return split;
    }
}
