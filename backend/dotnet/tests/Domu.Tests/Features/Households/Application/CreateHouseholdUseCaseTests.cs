using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class CreateHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesHouseholdAndPersistsIt()
    {
        var repository = new FakeHouseholdRepository();
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var useCase = new CreateHouseholdUseCase(repository, membershipRepository);
        var ownerMemberId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateHouseholdCommand(new DomuActor(ownerMemberId, DomuActorType.Zitadel), "Home", "Alex"),
            CancellationToken.None);

        Assert.Equal("Home", result.Name);
        Assert.Equal(HouseholdSubscriptionPlan.Free, result.SubscriptionPlan);
        Assert.Equal(HouseholdSubscriptionStatus.Active, result.SubscriptionStatus);
        Assert.Equal(result.Id, repository.StoredHouseholds.Single().Id);
        Assert.Equal(result.Id, membershipRepository.Members.Single().HouseholdId);
        Assert.Equal("Alex", membershipRepository.Members.Single().DisplayName);
        Assert.Equal(ownerMemberId, membershipRepository.Members.Single().UserId);
        Assert.NotEqual(ownerMemberId, membershipRepository.Members.Single().Id);
        Assert.Equal(HouseholdMemberRole.Owner, membershipRepository.Members.Single().Role);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    private sealed class FakeHouseholdRepository : IHouseholdRepository
    {
        public List<Household> StoredHouseholds { get; } = [];
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredHouseholds.SingleOrDefault(household => household.Id == householdId));
        }

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(StoredHouseholds);
        }

        public Task AddAsync(Household household, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredHouseholds.Add(household);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household household, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            StoredHouseholds.RemoveAll(household => household.Id == householdId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }

    }
}
