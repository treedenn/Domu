namespace Domu.Api.Features.Households.Domain.Members;

public sealed class HouseholdMember
{
    public HouseholdMember(
        Guid id,
        Guid householdId,
        Guid userId,
        HouseholdMemberRole role,
        DateTimeOffset joinedAt)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household member id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        UserId = userId == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(userId))
            : userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public Guid UserId { get; }
    public HouseholdMemberRole Role { get; }
    public DateTimeOffset JoinedAt { get; }
}
