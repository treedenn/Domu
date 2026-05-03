namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record DeleteSpaceCommand(Guid UserId, Guid HouseholdId, Guid SpaceId);
