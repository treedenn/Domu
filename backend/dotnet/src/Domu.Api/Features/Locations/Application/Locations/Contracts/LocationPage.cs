namespace Domu.Api.Features.Locations.Application.Locations.Contracts;

public sealed record LocationPage(
    IReadOnlyList<LocationView> Locations,
    int PageNumber,
    int PageSize,
    int TotalCount);
