using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record GetSpaceItemsQuery(DomuActor Actor, Guid HouseholdId, Guid SpaceId);