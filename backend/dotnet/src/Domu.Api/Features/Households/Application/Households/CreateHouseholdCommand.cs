namespace Domu.Api.Features.Households.Application.Households;

public sealed record CreateHouseholdCommand(Guid OwnerId, string Name, string OwnerDisplayName);
