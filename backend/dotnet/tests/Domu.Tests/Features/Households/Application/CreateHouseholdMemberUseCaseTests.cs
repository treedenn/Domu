using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class CreateHouseholdMemberUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesAccountlessMemberForOwner()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var useCase = new CreateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            membershipRepository);

        var result = await useCase.ExecuteAsync(
            new CreateHouseholdMemberCommand(household.Id, ownerId, " Alex ", HouseholdMemberRole.Member),
            CancellationToken.None);

        Assert.Equal("Alex", result.DisplayName);
        Assert.Null(result.UserId);
        Assert.Equal(HouseholdMemberRole.Member, result.Role);
        Assert.Single(membershipRepository.Members);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequesterIsNotOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var useCase = new CreateHouseholdMemberUseCase(
            new StubHouseholdRepository(household),
            new FakeHouseholdMembershipRepository());

        var action = () => useCase.ExecuteAsync(
            new CreateHouseholdMemberCommand(household.Id, Guid.NewGuid(), "Alex", HouseholdMemberRole.Member),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
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
