using System.Net.Mail;

namespace Domu.Api.Features.Households.Domain.Members;

public sealed class HouseholdInvitation
{
    public const int EmailMaxLength = 320;
    public const int TokenMaxLength = 128;
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(7);

    private string _email = null!;
    private string _token = null!;

    public HouseholdInvitation(
        Guid id,
        Guid householdId,
        string email,
        string displayName,
        Guid invitedByMemberId,
        HouseholdMemberRole role,
        string token,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household invitation id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        InvitedByMemberId = invitedByMemberId == Guid.Empty
            ? throw new ArgumentException("Invited-by member id cannot be empty.", nameof(invitedByMemberId))
            : invitedByMemberId;
        if (!Enum.IsDefined(role))
            throw new ArgumentException("Invitation role is invalid.", nameof(role));
        if (role == HouseholdMemberRole.Unspecified)
            throw new ArgumentException("Invitation role must be specified.", nameof(role));
        if (role == HouseholdMemberRole.Owner)
            throw new ArgumentException("Invitations cannot grant owner role.", nameof(role));
        Role = role;
        Email = NormalizeEmail(email);
        DisplayName = HouseholdMember.ValidateDisplayName(displayName);
        Token = ValidateToken(token);
        CreatedAt = createdAt;
        ExpiresAt = expiresAt > createdAt
            ? expiresAt
            : throw new ArgumentException("Invitation expiry must be after creation time.", nameof(expiresAt));
    }

    public HouseholdInvitation(
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
        : this(id, householdId, email, displayName, invitedByMemberId, role, token, createdAt, expiresAt)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException("Invitation status is invalid.", nameof(status));
        if (status == HouseholdInvitationStatus.Unknown)
            throw new ArgumentException("Invitation status must be specified.", nameof(status));
        if (status == HouseholdInvitationStatus.Accepted && acceptedAt is null)
            throw new ArgumentException("Accepted invitations must have an accepted time.", nameof(acceptedAt));
        if (status != HouseholdInvitationStatus.Accepted && acceptedAt is not null)
            throw new ArgumentException("Only accepted invitations can have an accepted time.", nameof(acceptedAt));

        Status = status;
        AcceptedAt = acceptedAt;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public string DisplayName { get; }
    public Guid InvitedByMemberId { get; }
    public HouseholdMemberRole Role { get; }
    public HouseholdInvitationStatus Status { get; private set; } = HouseholdInvitationStatus.Pending;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? AcceptedAt { get; private set; }

    public string Email
    {
        get => _email;
        private set => _email = value;
    }

    public string Token
    {
        get => _token;
        private set => _token = value;
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        if (Status != HouseholdInvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be accepted.");
        if (acceptedAt > ExpiresAt)
            throw new InvalidOperationException("Expired invitations cannot be accepted.");

        Status = HouseholdInvitationStatus.Accepted;
        AcceptedAt = acceptedAt;
    }

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (normalizedEmail.Length > EmailMaxLength)
            throw new ArgumentException($"Email cannot be longer than {EmailMaxLength} characters.", nameof(email));

        try
        {
            var address = new MailAddress(normalizedEmail);
            if (!string.Equals(address.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email must be a valid email address.", nameof(email));
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Email must be a valid email address.", nameof(email), exception);
        }

        return normalizedEmail;
    }

    private static string ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Invitation token cannot be empty.", nameof(token));
        if (token.Length > TokenMaxLength)
            throw new ArgumentException($"Invitation token cannot be longer than {TokenMaxLength} characters.", nameof(token));

        return token;
    }
}
