using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class CreateHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesHouseholdAndPersistsIt()
    {
        var repository = new FakeHouseholdRepository();
        var useCase = new CreateHouseholdUseCase(repository);
        var ownerId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateHouseholdCommand(ownerId, "Home"),
            CancellationToken.None);

        Assert.Equal(ownerId, result.OwnerId);
        Assert.Equal("Home", result.Name);
        Assert.Equal(HouseholdSubscriptionPlan.Free, result.SubscriptionPlan);
        Assert.Equal(HouseholdSubscriptionStatus.Active, result.SubscriptionStatus);
        Assert.Equal(result.Id, repository.StoredHouseholds.Single().Id);
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

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                StoredHouseholds.Where(household => household.OwnerId == ownerId).ToArray());
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
