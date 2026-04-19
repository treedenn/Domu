using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Api.Features.Locations.Infrastructure.Locations;

public sealed class LocationEntity
{
    private LocationEntity()
    {
    }

    public LocationEntity(Guid id, Guid ownerId, string name, string? description, Guid? parentId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Location id cannot be empty.", nameof(id))
            : id;
        OwnerId = ownerId == Guid.Empty
            ? throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId))
            : ownerId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Location name cannot be empty.", nameof(name))
            : name;
        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent location id cannot be empty.", nameof(parentId));

        Description = description;
        ParentId = parentId;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }

    public Location ToDomain()
    {
        var location = new Location(Id, Name, OwnerId);
        location.Describe(Description);
        location.MoveTo(ParentId);
        return location;
    }

    public static LocationEntity FromDomain(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return new LocationEntity(
            location.Id,
            location.OwnerId,
            location.Name,
            location.Description,
            location.ParentId);
    }

    public void UpdateFromDomain(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (location.Id != Id)
            throw new ArgumentException("Cannot update location entity from a different location.", nameof(location));

        OwnerId = location.OwnerId;
        Name = location.Name;
        Description = location.Description;
        ParentId = location.ParentId;
    }
}
