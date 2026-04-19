using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Infrastructure.Items;

public sealed class ItemEntity
{
    private readonly List<ItemEntryEntity> _entries = [];

    private ItemEntity()
    {
    }

    public ItemEntity(Guid id, Guid locationId, string name, string? category, string? barcode)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Item id cannot be empty.", nameof(id))
            : id;
        LocationId = locationId == Guid.Empty
            ? throw new ArgumentException("Location id cannot be empty.", nameof(locationId))
            : locationId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Item name cannot be empty.", nameof(name))
            : name;
        Category = category;
        Barcode = barcode;
    }

    public Guid Id { get; private set; }
    public Guid LocationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Category { get; private set; }
    public string? Barcode { get; private set; }
    public IReadOnlyCollection<ItemEntryEntity> Entries => _entries;

    public Item ToDomain()
    {
        var item = new Item(Id, Name, LocationId);
        item.ChangeCategory(Category);
        item.ChangeBarcode(Barcode);

        foreach (var entry in _entries.Select(entry => entry.ToDomain()))
            item.AddEntry(entry);

        return item;
    }

    public static ItemEntity FromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = new ItemEntity(item.Id, item.LocationId, item.Name, item.Category, item.Barcode);
        foreach (var entry in item.Entries.OrderBy(entry => entry.Id))
            entity._entries.Add(ItemEntryEntity.FromDomain(entry));

        return entity;
    }

    public void UpdateFromDomain(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id != Id)
            throw new ArgumentException("Cannot update item entity from a different item.", nameof(item));

        LocationId = item.LocationId;
        Name = item.Name;
        Category = item.Category;
        Barcode = item.Barcode;

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
