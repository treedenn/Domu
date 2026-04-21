namespace Domu.Api.Features.Households.Application.Households;

public sealed record UpdateHouseholdCommand(Guid HouseholdId, Guid OwnerId, string Name);
