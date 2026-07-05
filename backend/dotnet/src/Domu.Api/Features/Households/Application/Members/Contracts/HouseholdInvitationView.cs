using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Contracts;

public sealed record HouseholdInvitationView(
    Guid Id,
    Guid HouseholdId,
    string Email,
    string DisplayName,
    Guid InvitedByUserId,
    HouseholdMemberRole Role,
    HouseholdInvitationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt)
{
    public static HouseholdInvitationView FromDomain(HouseholdInvitation invitation)
    {
        return new HouseholdInvitationView(
            invitation.Id,
            invitation.HouseholdId,
            invitation.Email,
            invitation.DisplayName,
            invitation.InvitedByUserId,
            invitation.Role,
            invitation.Status,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.AcceptedAt);
    }
}
