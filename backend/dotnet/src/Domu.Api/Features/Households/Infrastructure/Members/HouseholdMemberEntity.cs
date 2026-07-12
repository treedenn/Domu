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
        string displayName,
        HouseholdMemberRole role,
        DateTimeOffset joinedAt,
        bool archived)
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
        DisplayName = HouseholdMember.ValidateDisplayName(displayName);
        Role = role;
        JoinedAt = joinedAt;
        Archived = archived;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public Guid UserId { get; }
    public string DisplayName { get; } = null!;
    public HouseholdMemberRole Role { get; }
    public DateTimeOffset JoinedAt { get; }
    public bool Archived { get; }

    public HouseholdMember ToDomain()
    {
        return new HouseholdMember(Id, HouseholdId, UserId, DisplayName, Role, JoinedAt, Archived);
    }

    public static HouseholdMemberEntity FromDomain(HouseholdMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        return new HouseholdMemberEntity(
            member.Id,
            member.HouseholdId,
            member.UserId,
            member.DisplayName,
            member.Role,
            member.JoinedAt,
            member.Archived);
    }
}