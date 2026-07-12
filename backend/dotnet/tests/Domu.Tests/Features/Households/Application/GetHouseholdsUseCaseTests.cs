using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsOnlyAccessibleHouseholds()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeHouseholdRepository(
            new Household(Guid.NewGuid(), "Home"),
            new Household(Guid.NewGuid(), "Other"));
        var useCase = new GetHouseholdsUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdsQuery(new DomuActor(userId, DomuActorType.Zitadel)), CancellationToken.None);

        var household = Assert.Single(result);
        Assert.Equal("Home", household.Name);
    }

    private sealed class FakeHouseholdRepository(params Household[] seededHouseholds) : IHouseholdRepository
    {
        private readonly List<Household> _storedHouseholds = seededHouseholds.ToList();

        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storedHouseholds.SingleOrDefault(household => household.Id == householdId));
        }

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(_storedHouseholds.Take(1).ToArray());
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
