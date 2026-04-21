using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Domain.Spaces;

// A space is a hierarchical container for items inside a household.
public sealed class Space
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 255;

    private readonly HashSet<Space> _children = [];
    private readonly HashSet<Item> _items = [];
    private string _name = null!;
    private string? _description;
    private Guid? _parentId;

    public Space(Guid id, string name, Guid householdId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Space id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        Rename(name);
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public string? Description => _description;
    public Guid? ParentId => _parentId;
    public IReadOnlySet<Space> Children => _children.AsReadOnly();
    public IReadOnlySet<Item> Items => _items.AsReadOnly();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Space name cannot be null or whitespace.", nameof(name));
        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Space name cannot be longer than {NameMaxLength} characters.", nameof(name));

        Name = name;
    }

    public void Describe(string? description)
    {
        if (description?.Length > DescriptionMaxLength)
            throw new ArgumentException(
                $"Space description cannot be longer than {DescriptionMaxLength} characters.",
                nameof(description));

        _description = description;
    }

    public void MoveTo(Guid? parentId)
    {
        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent space id cannot be empty.", nameof(parentId));
        if (parentId == Id)
            throw new ArgumentException("Parent space cannot be itself.", nameof(parentId));

        _parentId = parentId;
    }

    public bool AddChild(Space child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Id == Id)
            throw new ArgumentException("A space cannot be a child of itself.", nameof(child));
        if (child.HouseholdId != HouseholdId)
            throw new ArgumentException("Child space must belong to the same household.", nameof(child));

        child.MoveTo(Id);
        return _children.Add(child);
    }

    public bool RemoveChild(Guid childId)
    {
        var child = _children.FirstOrDefault(existingChild => existingChild.Id == childId);
        if (child is null)
            return false;

        child.MoveTo(null);
        return _children.Remove(child);
    }

    public bool AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.SpaceId != Id)
            throw new ArgumentException("Item must belong to this space.", nameof(item));

        return _items.Add(item);
    }

    public bool RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(existingItem => existingItem.Id == itemId);
        if (item is null)
            return false;

        return _items.Remove(item);
    }
}
