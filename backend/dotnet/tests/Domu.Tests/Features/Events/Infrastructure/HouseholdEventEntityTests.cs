using Domu.Api.Features.Events.Domain;
using Domu.Api.Features.Events.Infrastructure;

namespace Domu.Tests.Features.Events.Infrastructure;

public sealed class HouseholdEventEntityTests
{
    [Fact]
    public void ToDomain_RoundTripsPersistedEventData()
    {
        var eventId = Guid.NewGuid();
        var actorMemberId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var userEvent = new HouseholdEvent(
            eventId,
            occurredAt,
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

        var entity = new HouseholdEventEntity(userEvent);
        var roundTripped = entity.ToDomain();

        Assert.Equal(eventId, roundTripped.Id);
        Assert.Equal(occurredAt, roundTripped.OccurredAt);
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
