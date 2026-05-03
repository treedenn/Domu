namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record DeleteItemCommand(Guid UserId, Guid HouseholdId, Guid SpaceId, Guid ItemId);
