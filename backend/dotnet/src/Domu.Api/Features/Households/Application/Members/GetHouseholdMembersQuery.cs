namespace Domu.Api.Features.Households.Application.Members;

public sealed record GetHouseholdMembersQuery(Guid HouseholdId, Guid UserId);
