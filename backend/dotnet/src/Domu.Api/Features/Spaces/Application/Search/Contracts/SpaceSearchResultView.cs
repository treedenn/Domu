namespace Domu.Api.Features.Spaces.Application.Search.Contracts;

public sealed record SpaceSearchResultView(
    Guid Id,
    Guid HouseholdId,
    Guid? ParentId,
    string Name,
    string? Description);