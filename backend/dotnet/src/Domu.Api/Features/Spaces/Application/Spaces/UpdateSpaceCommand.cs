namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record UpdateSpaceCommand(
    Guid UserId,
    Guid HouseholdId,
    Guid SpaceId,
    string Name,
    string? Description);
