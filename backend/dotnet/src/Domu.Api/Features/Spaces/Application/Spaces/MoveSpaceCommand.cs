namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record MoveSpaceCommand(Guid SpaceId, Guid? ParentId);
