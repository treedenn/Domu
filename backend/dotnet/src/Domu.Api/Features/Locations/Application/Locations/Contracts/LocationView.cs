using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Api.Features.Locations.Application.Locations.Contracts;

public sealed record LocationView(
    Guid Id,
    Guid OwnerId,
    Guid? ParentId,
    string Name,
    string? Description,
    int? ItemCount,
    IReadOnlyList<LocationItemView>? Items,
    int? ChildLocationCount,
    IReadOnlyList<LocationChildView>? ChildLocations)
{
    public static LocationView FromDomain(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return new LocationView(
            location.Id,
            location.OwnerId,
            location.ParentId,
            location.Name,
            location.Description,
            null,
            null,
            null,
            null);
    }
}
