using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Application.Spaces.Contracts;

public sealed record SpaceView(
    Guid Id,
    Guid HouseholdId,
    Guid? ParentId,
    string Name,
    string? Description,
    CollectionView<SpaceItemView>? Items,
    CollectionView<SpaceChildView>? ChildSpaces)
{
    public static SpaceView FromDomain(Space space)
    {
        ArgumentNullException.ThrowIfNull(space);

        return new SpaceView(
            space.Id,
            space.HouseholdId,
            space.ParentId,
            space.Name,
            space.Description,
            null,
            null);
    }
}