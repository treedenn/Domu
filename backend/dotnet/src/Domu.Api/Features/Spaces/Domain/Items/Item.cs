namespace Domu.Api.Features.Spaces.Domain.Items;

// Item is a physical object that can be stored in a space.
public sealed class Item
{
    public const int NameMaxLength = 255;
    public const int CategoryMaxLength = 255;
    public const int BarcodeMaxLength = 128;

    private readonly HashSet<ItemEntry> _entries = [];
    private string _name = null!;
    private string? _category;
    private string? _barcode;

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
    public int TotalQuantity => _entries.Sum(entry => entry.Quantity);

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public string? Category => _category;
    public string? Barcode => _barcode;

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

        _category = category;
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

        _barcode = barcode;
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
}
