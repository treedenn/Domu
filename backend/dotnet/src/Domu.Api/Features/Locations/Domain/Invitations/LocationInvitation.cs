namespace Domu.Api.Features.Locations.Domain.Invitations;

public sealed class LocationInvitation
{
    public LocationInvitation(
        Guid id,
        Guid locationId,
        Guid invitedByUserId,
        string email,
        string token,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt
    )
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Invitation id cannot be empty.") : id;
        LocationId = locationId == Guid.Empty
            ? throw new ArgumentException("Location id cannot be empty.")
            : locationId;
        InvitedByUserId = invitedByUserId == Guid.Empty
            ? throw new ArgumentException("Invited by user id cannot be empty.")
            : invitedByUserId;
        Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("Email is required.") : email;
        Token = string.IsNullOrWhiteSpace(token) ? throw new ArgumentException("Token is required.") : token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        Status = LocationInvitationStatus.Pending;
    }

    public Guid Id { get; }
    public Guid LocationId { get; }
    public Guid InvitedByUserId { get; }
    public string Email { get; }
    public string Token { get; }
    public LocationInvitationStatus Status { get; private set; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset CreatedAt { get; }
    
    public bool IsPending => Status == LocationInvitationStatus.Pending;
    
    public void Accept()
    {
        if (!IsPending)
            throw new InvalidOperationException("Invitation must be pending to be accepted. Current status: " + Status + ".");
        Status = LocationInvitationStatus.Accepted;
    }

    public void Revoke()
    {
        if (!IsPending)
            throw new InvalidOperationException("Invitation must be pending to be revoked. Current status: " + Status + ".");
        Status = LocationInvitationStatus.Revoked;
    }

    public void Expire()
    {
        if (!IsPending)
            throw new InvalidOperationException("Invitation must be pending to be expired. Current status: " + Status + ".");
        Status = LocationInvitationStatus.Expired;
    }
}