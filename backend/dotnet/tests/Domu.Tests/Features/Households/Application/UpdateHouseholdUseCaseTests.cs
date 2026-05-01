using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class UpdateHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingOwnedHousehold()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new UpdateHouseholdUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new UpdateHouseholdCommand(household.Id, ownerId, "Apartment"),
            CancellationToken.None);

        Assert.Equal("Apartment", result.Name);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHouseholdBelongsToAnotherOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new UpdateHouseholdUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new UpdateHouseholdCommand(household.Id, Guid.NewGuid(), "Apartment"),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal(0, repository.UpdateCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    private sealed class FakeHouseholdRepository(params Household[] seededHouseholds) : IHouseholdRepository
    {
        private readonly List<Household> _storedHouseholds = seededHouseholds.ToList();

        public int UpdateCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storedHouseholds.SingleOrDefault(household => household.Id == householdId));
        }

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _storedHouseholds.Where(household => household.OwnerId == ownerId).ToArray());
        }

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return GetByOwnerIdAsync(userId, cancellationToken);
        }

        public Task AddAsync(Household household, CancellationToken cancellationToken)
        {
            _storedHouseholds.Add(household);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household household, CancellationToken cancellationToken)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            _storedHouseholds.RemoveAll(household => household.Id == householdId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
