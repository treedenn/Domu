// A location is a container for items and locations.
// e.g., household, office, warehouse, room, shelf, box, etc.

using Domu.Api.Features.Locations.Domain.Items;
using Domu.Api.Features.Locations.Domain.Membership;

namespace Domu.Api.Features.Locations.Domain.Locations;

public class Location
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 255;
    private readonly HashSet<Location> _children = [];
    private readonly HashSet<Item> _items = [];
    private readonly HashSet<LocationMember> _members = [];
    private string _name = null!;
    private string? _description;
    private Guid? _parentId;

    public Location(Guid id, string name, Guid ownerId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Location id cannot be empty.")
            : id;
        OwnerId = ownerId == Guid.Empty
            ? throw new ArgumentException("Owner id cannot be empty.")
            : ownerId;
        Rename(name);
    }

    public Guid Id { get; }
    public Guid OwnerId { get; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public string? Description => _description;

    public Guid? ParentId => _parentId;

    public IReadOnlySet<Location> Children => _children.AsReadOnly();
    public IReadOnlySet<Item> Items => _items.AsReadOnly();
    public IReadOnlySet<LocationMember> Members => _members.AsReadOnly();

    public bool IsOwner(Guid userId)
    {
        return OwnerId == userId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be null or whitespace.", nameof(name));
        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Location name cannot be longer than {NameMaxLength} characters.",
                nameof(name));

        Name = name;
    }

    public void Describe(string? description)
    {
        if (description?.Length > DescriptionMaxLength)
            throw new ArgumentException(
                $"Location description cannot be longer than {DescriptionMaxLength} characters.",
                nameof(description));

        _description = description;
    }

    public void MoveTo(Guid? parentId)
    {
        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent location id cannot be empty.", nameof(parentId));
        if (parentId == Id)
            throw new ArgumentException("Parent location cannot be itself.", nameof(parentId));

        _parentId = parentId;
    }

    public bool AddChild(Location child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Id == Id)
            throw new ArgumentException("A location cannot be a child of itself.", nameof(child));

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

        if (item.LocationId != Id)
            throw new ArgumentException("Item must belong to this location.", nameof(item));

        return _items.Add(item);
    }

    public bool RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(existingItem => existingItem.Id == itemId);
        if (item is null)
            return false;

        return _items.Remove(item);
    }

    public bool AddMember(LocationMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (member.LocationId != Id)
            throw new ArgumentException("Member must belong to this location.", nameof(member));
        if (member.UserId == OwnerId)
            throw new InvalidOperationException("Owner should not be added as a location member.");

        return _members.Add(member);
    }

    public bool RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(existingMember => existingMember.UserId == userId);
        if (member is null)
            return false;

        return _members.Remove(member);
    }

    public bool IsMember(Guid userId)
    {
        return IsOwner(userId) || Members.Any(m => m.UserId == userId);
    }
}