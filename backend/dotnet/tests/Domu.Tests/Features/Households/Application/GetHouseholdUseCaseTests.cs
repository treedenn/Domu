using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsExistingOwnedHousehold()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new GetHouseholdUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdQuery(household.Id, ownerId),
            CancellationToken.None);

        Assert.Equal(household.Id, result.Id);
        Assert.Equal(ownerId, result.OwnerId);
        Assert.Equal("Home", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHouseholdBelongsToAnotherOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new GetHouseholdUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new GetHouseholdQuery(household.Id, Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeHouseholdRepository(params Household[] seededHouseholds) : IHouseholdRepository
    {
        private readonly List<Household> _storedHouseholds = seededHouseholds.ToList();

        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storedHouseholds.SingleOrDefault(household => household.Id == householdId));
        }

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _storedHouseholds.Where(household => household.OwnerId == ownerId).ToArray());
        }

        public Task AddAsync(Household household, CancellationToken cancellationToken)
        {
            _storedHouseholds.Add(household);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household household, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            _storedHouseholds.RemoveAll(household => household.Id == householdId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
