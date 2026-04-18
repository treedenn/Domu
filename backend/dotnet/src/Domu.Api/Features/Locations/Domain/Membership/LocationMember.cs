namespace Domu.Api.Features.Locations.Domain.Membership;

public sealed class LocationMember
{
    public LocationMember(Guid userId, Guid locationId, MembershipRole role)
    {
        UserId = userId == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(userId))
            : userId;
        LocationId = locationId == Guid.Empty
            ? throw new ArgumentException("Location id cannot be empty.", nameof(locationId))
            : locationId;
        Role = role;
    }

    public Guid UserId { get; }
    public Guid LocationId { get; }
    public MembershipRole Role { get; private set; }

    public void ChangeRole(MembershipRole role)
    {
        Role = role;
    }
}