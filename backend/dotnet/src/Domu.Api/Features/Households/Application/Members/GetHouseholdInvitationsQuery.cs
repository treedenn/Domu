namespace Domu.Api.Features.Households.Application.Members;

public sealed record GetHouseholdInvitationsQuery(Guid HouseholdId, Guid UserId);
