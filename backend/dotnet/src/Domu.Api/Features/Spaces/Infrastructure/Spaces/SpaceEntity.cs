using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Infrastructure.Spaces;

public sealed class SpaceEntity
{
    private SpaceEntity()
    {
    }

    public SpaceEntity(Guid id, Guid householdId, string name, string? description, Guid? parentId)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Space id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Space name cannot be empty.", nameof(name))
            : name;
        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent space id cannot be empty.", nameof(parentId));

        Description = description;
        ParentId = parentId;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }

    public Space ToDomain()
    {
        var space = new Space(Id, Name, HouseholdId);
        space.Describe(Description);
        space.MoveTo(ParentId);
        return space;
    }

    public static SpaceEntity FromDomain(Space space)
    {
        ArgumentNullException.ThrowIfNull(space);

        return new SpaceEntity(
            space.Id,
            space.HouseholdId,
            space.Name,
            space.Description,
            space.ParentId);
    }

    public void UpdateFromDomain(Space space)
    {
        ArgumentNullException.ThrowIfNull(space);
        if (space.Id != Id)
            throw new ArgumentException("Cannot update space entity from a different space.", nameof(space));

        HouseholdId = space.HouseholdId;
        Name = space.Name;
        Description = space.Description;
        ParentId = space.ParentId;
    }
}