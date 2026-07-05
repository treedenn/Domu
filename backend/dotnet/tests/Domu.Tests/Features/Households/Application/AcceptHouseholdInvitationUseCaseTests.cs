using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class AcceptHouseholdInvitationUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_AcceptsPendingInvitationAndCreatesMember()
    {
        var repository = new FakeHouseholdMembershipRepository();
        var now = DateTimeOffset.UtcNow;
        var invitation = new HouseholdInvitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "person@example.com",
            "Alex",
            Guid.NewGuid(),
            HouseholdMemberRole.Admin,
            "token",
            now,
            now.AddDays(1));
        await repository.AddInvitationAsync(invitation, CancellationToken.None);
        var userId = Guid.NewGuid();
        var useCase = new AcceptHouseholdInvitationUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new AcceptHouseholdInvitationCommand("token", userId),
            CancellationToken.None);

        Assert.Equal(invitation.HouseholdId, result.HouseholdId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Alex", result.DisplayName);
        Assert.Equal(HouseholdMemberRole.Admin, result.Role);
        Assert.Equal(HouseholdInvitationStatus.Accepted, repository.Invitations.Single().Status);
        Assert.Single(repository.Members);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvitationIsExpired_Throws()
    {
        var repository = new FakeHouseholdMembershipRepository();
        var now = DateTimeOffset.UtcNow;
        await repository.AddInvitationAsync(
            new HouseholdInvitation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "person@example.com",
                "Alex",
                Guid.NewGuid(),
                HouseholdMemberRole.Member,
                "token",
                now.AddDays(-2),
                now.AddDays(-1)),
            CancellationToken.None);
        var useCase = new AcceptHouseholdInvitationUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new AcceptHouseholdInvitationCommand("token", Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }
}
