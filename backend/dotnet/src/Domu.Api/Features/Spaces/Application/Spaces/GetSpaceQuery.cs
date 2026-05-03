namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record GetSpaceQuery(Guid UserId, Guid HouseholdId, Guid SpaceId);
