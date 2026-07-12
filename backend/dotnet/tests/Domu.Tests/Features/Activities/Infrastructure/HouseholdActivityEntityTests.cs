using Domu.Api.Features.Activities.Domain;
using Domu.Api.Features.Activities.Infrastructure;

namespace Domu.Tests.Features.Activities.Infrastructure;

public sealed class HouseholdActivityEntityTests
{
    [Fact]
    public void ToDomain_RoundTripsPersistedActivityData()
    {
        var activityId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var actorMemberId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var householdActivity = new HouseholdActivity(
            activityId,
            occurredAt,
            actorId,
            actorMemberId,
            "item.created",
            "item",
            targetId,
            householdId,
            """{"name":"Milk"}""",
            "request-1",
            "mobile",
            "ios",
            "1.2.3",
            42);

        var entity = new HouseholdActivityEntity(householdActivity);
        var roundTripped = entity.ToDomain();

        Assert.Equal(activityId, roundTripped.Id);
        Assert.Equal(occurredAt, roundTripped.OccurredAt);
        Assert.Equal(actorId, roundTripped.ActorId);
        Assert.Equal(actorMemberId, roundTripped.ActorMemberId);
        Assert.Equal("item.created", roundTripped.Action);
        Assert.Equal("item", roundTripped.TargetType);
        Assert.Equal(targetId, roundTripped.TargetId);
        Assert.Equal(householdId, roundTripped.HouseholdId);
        Assert.Equal("""{"name":"Milk"}""", roundTripped.MetadataJson);
        Assert.Equal("request-1", roundTripped.RequestId);
        Assert.Equal("mobile", roundTripped.ClientApp);
        Assert.Equal("ios", roundTripped.ClientPlatform);
        Assert.Equal("1.2.3", roundTripped.ClientVersion);
        Assert.Equal(42, roundTripped.ClientBuild);
    }
}
