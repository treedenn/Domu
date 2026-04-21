namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record UpdateSpaceCommand(
    Guid SpaceId,
    string Name,
    string? Description,
    Guid? ParentId);
