namespace Domu.Api.Features.Households.Application.Households;

public sealed record DeleteHouseholdCommand(Guid HouseholdId, Guid OwnerId);
