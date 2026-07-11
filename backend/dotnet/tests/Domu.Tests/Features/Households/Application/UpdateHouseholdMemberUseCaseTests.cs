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
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var member = new HouseholdMember(
            Guid.NewGuid(),
            household.Id,
            null,
            "Alex",
            HouseholdMemberRole.Member,
            DateTimeOffset.UtcNow);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        await membershipRepository.AddMemberAsync(member, CancellationToken.None);
        var useCase = new UpdateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            membershipRepository);

        var result = await useCase.ExecuteAsync(
            new UpdateHouseholdMemberCommand(
                household.Id,
                member.Id,
                ownerId,
                " Sam ",
                HouseholdMemberRole.Admin,
                Archived: true),
            CancellationToken.None);

        Assert.Equal("Sam", result.DisplayName);
        Assert.Equal(HouseholdMemberRole.Admin, result.Role);
        Assert.True(result.Archived);
        var storedMember = Assert.Single(membershipRepository.Members);
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
            null,
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
                household.Id,
                member.Id,
                Guid.NewGuid(),
                "Sam",
                HouseholdMemberRole.Admin,
                Archived: false),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberIsOwner_Throws()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var member = new HouseholdMember(
            Guid.NewGuid(),
            household.Id,
            ownerId,
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
                household.Id,
                member.Id,
                ownerId,
                "Owner",
                HouseholdMemberRole.Admin,
                Archived: false),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private sealed class StubHouseholdRepository(Household household) : IHouseholdRepository
    {
        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken) =>
            Task.FromResult(household.Id == householdId ? household : null);

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Household>>([]);

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Household>>([]);

        public Task AddAsync(Household value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Household value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
