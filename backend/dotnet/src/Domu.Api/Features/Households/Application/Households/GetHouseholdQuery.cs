namespace Domu.Api.Features.Households.Application.Households;

public sealed record GetHouseholdQuery(Guid HouseholdId, Guid OwnerId);
