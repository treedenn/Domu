using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class DeleteHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesExistingOwnedHousehold()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new DeleteHouseholdUseCase(repository);

        await useCase.ExecuteAsync(new DeleteHouseholdCommand(household.Id, ownerId), CancellationToken.None);

        Assert.Empty(repository.StoredHouseholds);
        Assert.Equal(1, repository.DeleteCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHouseholdBelongsToAnotherOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new DeleteHouseholdUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new DeleteHouseholdCommand(household.Id, Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal(0, repository.DeleteCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    private sealed class FakeHouseholdRepository(params Household[] seededHouseholds) : IHouseholdRepository
    {
        public List<Household> StoredHouseholds { get; } = seededHouseholds.ToList();
        public int DeleteCalls { get; private set; }
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
            StoredHouseholds.Add(household);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household household, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            DeleteCalls++;
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
