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
}
