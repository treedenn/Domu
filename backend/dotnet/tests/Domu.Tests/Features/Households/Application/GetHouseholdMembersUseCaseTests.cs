using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdMembersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsActiveMembersAndOwnerManagementPermission()
    {
        var ownerUserId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), "Home");
        var householdRepository = new FakeHouseholdRepository(household);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var now = DateTimeOffset.UtcNow;

        await membershipRepository.AddMemberAsync(
            new HouseholdMember(Guid.NewGuid(), household.Id, ownerUserId, "Owner", HouseholdMemberRole.Owner, now),
            CancellationToken.None);
        await membershipRepository.AddMemberAsync(
            new HouseholdMember(Guid.NewGuid(), household.Id, Guid.NewGuid(), "Former member", HouseholdMemberRole.Member, now, archived: true),
            CancellationToken.None);
        var useCase = new GetHouseholdMembersUseCase(householdRepository, membershipRepository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdMembersQuery(new DomuActor(ownerUserId, DomuActorType.Zitadel), household.Id),
            CancellationToken.None);

        Assert.True(result.CanManageMembers);
        Assert.Collection(result.Members, member => Assert.Equal("Owner", member.DisplayName));
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotGrantManagementPermissionToNonOwner()
    {
        var ownerUserId = Guid.NewGuid();
        var memberUserId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), "Home");
        var householdRepository = new FakeHouseholdRepository(household);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var now = DateTimeOffset.UtcNow;

        await membershipRepository.AddMemberAsync(
            new HouseholdMember(Guid.NewGuid(), household.Id, ownerUserId, "Owner", HouseholdMemberRole.Owner, now),
            CancellationToken.None);
        await membershipRepository.AddMemberAsync(
            new HouseholdMember(Guid.NewGuid(), household.Id, memberUserId, "Member", HouseholdMemberRole.Member, now),
            CancellationToken.None);
        var useCase = new GetHouseholdMembersUseCase(householdRepository, membershipRepository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdMembersQuery(new DomuActor(memberUserId, DomuActorType.Zitadel), household.Id),
            CancellationToken.None);

        Assert.False(result.CanManageMembers);
    }

    private sealed class FakeHouseholdRepository(params Household[] households) : IHouseholdRepository
    {
        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken) =>
            Task.FromResult(households.SingleOrDefault(household => household.Id == householdId));

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Household>>(households);

        public Task AddAsync(Household household, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Household household, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
