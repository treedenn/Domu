using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Infrastructure.Items;

public sealed class ItemEntity
{
    private readonly List<ItemEntryEntity> _entries = [];

    private ItemEntity()
    {
    }

    public ItemEntity(Guid id, Guid spaceId, string name, string? category, string? barcode, int? defaultPurchaseCount = null, decimal? defaultPurchaseAmountPerUnit = null, ItemUnit? defaultPurchaseUnit = null)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Item id cannot be empty.", nameof(id))
            : id;
        SpaceId = spaceId == Guid.Empty
            ? throw new ArgumentException("Space id cannot be empty.", nameof(spaceId))
            : spaceId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Item name cannot be empty.", nameof(name))
            : name;
        Category = category;
        Barcode = barcode;
        DefaultPurchaseCount = defaultPurchaseCount;
        DefaultPurchaseAmountPerUnit = defaultPurchaseAmountPerUnit;
        DefaultPurchaseUnit = defaultPurchaseUnit;
    }

    public Guid Id { get; }
    public Guid SpaceId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Category { get; private set; }
    public string? Barcode { get; private set; }
    public int? DefaultPurchaseCount { get; private set; }
    public decimal? DefaultPurchaseAmountPerUnit { get; private set; }
    public ItemUnit? DefaultPurchaseUnit { get; private set; }
    public IReadOnlyCollection<ItemEntryEntity> Entries => _entries;

    public Item ToDomain()
    {
        var item = new Item(Id, Name, SpaceId);
        item.ChangeCategory(Category);
        item.ChangeBarcode(Barcode);
        item.SetDefaultPurchase(DefaultPurchaseCount, DefaultPurchaseAmountPerUnit, DefaultPurchaseUnit);

        foreach (var entry in _entries.Select(entry => entry.ToDomain()))
            item.AddEntry(entry);

        return item;
    }

    public static ItemEntity FromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = new ItemEntity(item.Id, item.SpaceId, item.Name, item.Category, item.Barcode, item.DefaultPurchaseCount, item.DefaultPurchaseAmountPerUnit, item.DefaultPurchaseUnit);
        foreach (var entry in item.Entries.OrderBy(entry => entry.Id))
            entity._entries.Add(ItemEntryEntity.FromDomain(entry));

        return entity;
    }

    public void UpdateFromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id != Id)
            throw new ArgumentException("Cannot update item entity from a different item.", nameof(item));

        SpaceId = item.SpaceId;
        Name = item.Name;
        Category = item.Category;
        Barcode = item.Barcode;
        DefaultPurchaseCount = item.DefaultPurchaseCount;
        DefaultPurchaseAmountPerUnit = item.DefaultPurchaseAmountPerUnit;
        DefaultPurchaseUnit = item.DefaultPurchaseUnit;

        var entriesById = _entries.ToDictionary(entry => entry.Id);
        var desiredEntryIds = item.Entries.Select(entry => entry.Id).ToHashSet();

        _entries.RemoveAll(existingEntry => !desiredEntryIds.Contains(existingEntry.Id));

        foreach (var entry in item.Entries.OrderBy(entry => entry.Id))
        {
            if (entriesById.TryGetValue(entry.Id, out var existingEntry))
            {
                existingEntry.UpdateFromDomain(entry);
                continue;
            }

            _entries.Add(ItemEntryEntity.FromDomain(entry));
        }
    }
}
