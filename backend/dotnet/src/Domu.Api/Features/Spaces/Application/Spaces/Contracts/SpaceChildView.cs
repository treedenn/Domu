namespace Domu.Api.Features.Spaces.Application.Spaces.Contracts;

public sealed record SpaceChildView(
    Guid Id,
    Guid HouseholdId,
    Guid? ParentId,
    string Name,
    string? Description);
