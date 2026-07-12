using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Activities.Infrastructure;
using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Domain.Members;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Infrastructure.Database;
using Domu.Api.Interface.RequestContext;
using Microsoft.EntityFrameworkCore;

namespace Domu.Tests.Features.Activities.Infrastructure;

public sealed class HouseholdActivityRecorderTests
{
    [Fact]
    public async Task RecordAsync_WithZitadelActor_StoresActorAndTrackedMemberIds()
    {
        await using var dbContext = CreateDbContext();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        dbContext.HouseholdMembers.Add(CreateMember(memberId, householdId, userId));
        var recorder = new HouseholdActivityRecorder(dbContext, new FakeClientRequestContextAccessor());

        await recorder.RecordAsync(
            new DomuActor(userId, DomuActorType.Zitadel),
            "item.created",
            "item",
            Guid.NewGuid(),
            householdId,
            ActivityMetadata.Empty(),
            CancellationToken.None);

        var activity = Assert.Single(dbContext.HouseholdActivities.Local).ToDomain();
        Assert.Equal(userId, activity.ActorId);
        Assert.Equal(memberId, activity.ActorMemberId);
        Assert.Equal(householdId, activity.HouseholdId);
    }

    [Fact]
    public async Task RecordAsync_WithHouseholdMemberActor_ResolvesTrackedMemberDirectly()
    {
        await using var dbContext = CreateDbContext();
        var householdId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        dbContext.HouseholdMembers.Add(CreateMember(memberId, householdId, Guid.NewGuid()));
        var recorder = new HouseholdActivityRecorder(dbContext, new FakeClientRequestContextAccessor());

        await recorder.RecordAsync(
            new DomuActor(memberId, DomuActorType.HouseholdMember),
            "space.updated",
            "space",
            Guid.NewGuid(),
            householdId,
            ActivityMetadata.Empty(),
            CancellationToken.None);

        var activity = Assert.Single(dbContext.HouseholdActivities.Local).ToDomain();
        Assert.Equal(memberId, activity.ActorId);
        Assert.Equal(memberId, activity.ActorMemberId);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=domu_activity_tests;Username=test;Password=test")
            .Options;
        return new AppDbContext(options);
    }

    private static HouseholdMemberEntity CreateMember(Guid id, Guid householdId, Guid userId)
    {
        return new HouseholdMemberEntity(
            id,
            householdId,
            userId,
            "Member",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow,
            false);
    }

    private sealed class FakeClientRequestContextAccessor : IClientRequestContextAccessor
    {
        public ClientRequestContext Current { get; set; } = new() { RequestId = "request-1" };
    }
}
