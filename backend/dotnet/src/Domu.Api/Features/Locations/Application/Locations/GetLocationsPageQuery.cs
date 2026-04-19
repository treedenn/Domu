namespace Domu.Api.Features.Locations.Application.Locations;

public sealed record GetLocationsPageQuery(
    Guid OwnerId,
    Guid? ParentId,
    int PageNumber = 1,
    int PageSize = 20,
    LocationItemsProjection Items = LocationItemsProjection.None,
    LocationChildrenProjection Children = LocationChildrenProjection.None);
