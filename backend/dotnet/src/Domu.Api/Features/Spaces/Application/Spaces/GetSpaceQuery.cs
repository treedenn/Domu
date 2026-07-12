using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record GetSpaceQuery(DomuActor Actor, Guid HouseholdId, Guid SpaceId);