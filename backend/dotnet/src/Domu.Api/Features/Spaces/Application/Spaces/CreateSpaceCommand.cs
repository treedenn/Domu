namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record CreateSpaceCommand(
    Guid HouseholdId,
    string Name,
    string? Description,
    Guid? ParentId);
