namespace Domu.Api.Features.Households.Domain.Members;

public sealed class HouseholdMember
{
    public const int DisplayNameMaxLength = 100;

    public HouseholdMember(
        Guid id,
        Guid householdId,
        Guid? userId,
        string displayName,
        HouseholdMemberRole role,
        DateTimeOffset joinedAt)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household member id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        UserId = userId is { } value && value == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(userId))
            : userId;
        DisplayName = ValidateDisplayName(displayName);
        if (!Enum.IsDefined(role))
            throw new ArgumentException("Household member role is invalid.", nameof(role));
        if (role == HouseholdMemberRole.Unspecified)
            throw new ArgumentException("Household member role must be specified.", nameof(role));
        Role = role;
        JoinedAt = joinedAt;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public Guid? UserId { get; }
    public string DisplayName { get; }
    public HouseholdMemberRole Role { get; }
    public DateTimeOffset JoinedAt { get; }

    public static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        var normalized = displayName.Trim();
        if (normalized.Length > DisplayNameMaxLength)
            throw new ArgumentException($"Display name cannot be longer than {DisplayNameMaxLength} characters.", nameof(displayName));

        return normalized;
    }
}
