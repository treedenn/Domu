using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class HouseholdMemberEntity
{
    private HouseholdMemberEntity()
    {
    }

    public HouseholdMemberEntity(
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

    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public Guid UserId { get; private set; }
    public HouseholdMemberRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public HouseholdMember ToDomain()
    {
        return new HouseholdMember(Id, HouseholdId, UserId, Role, JoinedAt);
    }

    public static HouseholdMemberEntity FromDomain(HouseholdMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        return new HouseholdMemberEntity(
            member.Id,
            member.HouseholdId,
            member.UserId,
            member.Role,
            member.JoinedAt);
    }
}
