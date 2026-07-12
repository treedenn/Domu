using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Domain;

public sealed class HouseholdMemberTests
{
    [Fact]
    public void Constructor_WithUserId_CreatesLinkedMember()
    {
        var userId = Guid.NewGuid();
        var member = new HouseholdMember(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            "Alex",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);

        Assert.Equal(userId, member.UserId);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_Throws()
    {
        var action = () => new HouseholdMember(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "Alex",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("User id cannot be empty.", exception.Message);
    }

    [Fact]
    public void Constructor_WithArchivedOwner_Throws()
    {
        var action = () => new HouseholdMember(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Owner",
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow,
            true);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("owner cannot be archived", exception.Message);
    }

    [Fact]
    public void ChangeRole_ForOwner_Throws()
    {
        var owner = CreateOwner();

        var action = () => owner.ChangeRole(HouseholdMemberRole.Admin);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("owner cannot be assigned another role", exception.Message);
    }

    [Fact]
    public void SetArchived_ForOwner_Throws()
    {
        var owner = CreateOwner();

        var action = () => owner.SetArchived(true);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("owner cannot be archived", exception.Message);
    }

    [Fact]
    public void ChangeRole_ToOwner_Throws()
    {
        var member = new HouseholdMember(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Member",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);

        var action = () => member.ChangeRole(HouseholdMemberRole.Owner);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("cannot be promoted to owner", exception.Message);
    }

    private static HouseholdMember CreateOwner()
    {
        return new HouseholdMember(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Owner",
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow);
    }
}
