using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record InviteHouseholdMemberCommand(
    Guid HouseholdId,
    Guid InvitedByUserId,
    string Email,
    HouseholdMemberRole Role);
