namespace Domu.Api.Features.Spaces.Application.Spaces.Contracts;

public sealed record SpacePage(
    IReadOnlyList<SpaceView> Spaces,
    int PageNumber,
    int PageSize,
    int TotalCount);
