using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class HouseholdInvitationEntity
{
    private HouseholdInvitationEntity()
    {
    }

    public HouseholdInvitationEntity(
        Guid id,
        Guid householdId,
        string email,
        string displayName,
        Guid invitedByMemberId,
        HouseholdMemberRole role,
        string token,
        HouseholdInvitationStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? acceptedAt)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household invitation id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        Email = string.IsNullOrWhiteSpace(email)
            ? throw new ArgumentException("Email cannot be empty.", nameof(email))
            : email;
        DisplayName = HouseholdMember.ValidateDisplayName(displayName);
        InvitedByMemberId = invitedByMemberId == Guid.Empty
            ? throw new ArgumentException("Invited-by member id cannot be empty.", nameof(invitedByMemberId))
            : invitedByMemberId;
        Role = role;
        Token = string.IsNullOrWhiteSpace(token)
            ? throw new ArgumentException("Token cannot be empty.", nameof(token))
            : token;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        AcceptedAt = acceptedAt;
    }

    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public Guid InvitedByMemberId { get; private set; }
    public HouseholdMemberRole Role { get; private set; }
    public string Token { get; private set; } = null!;
    public HouseholdInvitationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }

    public HouseholdInvitation ToDomain()
    {
        return new HouseholdInvitation(
            Id,
            HouseholdId,
            Email,
            DisplayName,
            InvitedByMemberId,
            Role,
            Token,
            Status,
            CreatedAt,
            ExpiresAt,
            AcceptedAt);
    }

    public static HouseholdInvitationEntity FromDomain(HouseholdInvitation invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        return new HouseholdInvitationEntity(
            invitation.Id,
            invitation.HouseholdId,
            invitation.Email,
            invitation.DisplayName,
            invitation.InvitedByMemberId,
            invitation.Role,
            invitation.Token,
            invitation.Status,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.AcceptedAt);
    }

    public void UpdateFromDomain(HouseholdInvitation invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        if (invitation.Id != Id)
            throw new ArgumentException("Cannot update invitation entity from a different invitation.", nameof(invitation));

        Status = invitation.Status;
        AcceptedAt = invitation.AcceptedAt;
    }
}
