using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsOnlyHouseholdsForOwner()
    {
        var ownerMemberId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var repository = new FakeHouseholdRepository(
            new Household(Guid.NewGuid(), ownerMemberId, "Home"),
            new Household(Guid.NewGuid(), otherOwnerId, "Other"));
        var useCase = new GetHouseholdsUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdsQuery(new DomuActor(ownerMemberId, DomuActorType.Zitadel)), CancellationToken.None);

        var household = Assert.Single(result);
        Assert.Equal(ownerMemberId, household.OwnerMemberId);
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
            return GetByOwnerIdAsync(userId, cancellationToken);
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

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerMemberId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _storedHouseholds.Where(household => household.OwnerMemberId == ownerMemberId).ToArray());
        }
    }
}