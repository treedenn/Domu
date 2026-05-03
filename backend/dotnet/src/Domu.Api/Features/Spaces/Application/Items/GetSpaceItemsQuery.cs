namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record GetSpaceItemsQuery(Guid UserId, Guid HouseholdId, Guid SpaceId);
