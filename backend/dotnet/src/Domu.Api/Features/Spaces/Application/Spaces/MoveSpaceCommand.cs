namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record MoveSpaceCommand(Guid UserId, Guid HouseholdId, Guid SpaceId, Guid? ParentId);
