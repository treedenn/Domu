using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class UpdateHouseholdMemberUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingMemberForOwner()
    {
        var ownerMemberId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerMemberId, "Home");
        var member = new HouseholdMember(
            Guid.NewGuid(),
            household.Id,
            Guid.NewGuid(),
            "Alex",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        await membershipRepository.AddMemberAsync(
            new HouseholdMember(ownerMemberId, household.Id, ownerUserId, "Owner", HouseholdMemberRole.Owner,
                DateTimeOffset.UtcNow), CancellationToken.None);
        await membershipRepository.AddMemberAsync(member, CancellationToken.None);
        var useCase = new UpdateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            membershipRepository);

        var result = await useCase.ExecuteAsync(
            new UpdateHouseholdMemberCommand(
                new DomuActor(ownerUserId, DomuActorType.Zitadel),
                household.Id,
                member.Id,
                " Sam ",
                HouseholdMemberRole.Admin,
                true),
            CancellationToken.None);

        Assert.Equal("Sam", result.DisplayName);
        Assert.Equal(HouseholdMemberRole.Admin, result.Role);
        Assert.True(result.Archived);
        var storedMember = Assert.Single(membershipRepository.Members, value => value.Id == member.Id);
        Assert.Equal("Sam", storedMember.DisplayName);
        Assert.Equal(HouseholdMemberRole.Admin, storedMember.Role);
        Assert.True(storedMember.Archived);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequesterIsNotOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var member = new HouseholdMember(
            Guid.NewGuid(),
            household.Id,
            Guid.NewGuid(),
            "Alex",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        await membershipRepository.AddMemberAsync(member, CancellationToken.None);
        var useCase = new UpdateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            membershipRepository);

        var action = () => useCase.ExecuteAsync(
            new UpdateHouseholdMemberCommand(
                new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
                household.Id,
                member.Id,
                "Sam",
                HouseholdMemberRole.Admin,
                false),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberIsOwner_Throws()
    {
        var ownerMemberId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerMemberId, "Home");
        var member = new HouseholdMember(
            ownerMemberId,
            household.Id,
            ownerUserId,
            "Owner",
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        await membershipRepository.AddMemberAsync(member, CancellationToken.None);
        var useCase = new UpdateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            membershipRepository);

        var action = () => useCase.ExecuteAsync(
            new UpdateHouseholdMemberCommand(
                new DomuActor(ownerUserId, DomuActorType.Zitadel),
                household.Id,
                member.Id,
                "Owner",
                HouseholdMemberRole.Admin,
                false),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private sealed class StubHouseholdRepository(Household household) : IHouseholdRepository
    {
        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(household.Id == householdId ? household : null);
        }

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>([]);
        }

        public Task AddAsync(Household value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerMemberId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>([]);
        }
    }
}