using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsExistingOwnedHousehold()
    {
        var ownerMemberId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerMemberId, "Home");
        var repository = new FakeHouseholdRepository(household);
        var memberships = new FakeHouseholdMembershipRepository();
        await memberships.AddMemberAsync(new HouseholdMember(ownerMemberId, household.Id, ownerUserId, "Owner", HouseholdMemberRole.Owner, DateTimeOffset.UtcNow), CancellationToken.None);
        var useCase = new GetHouseholdUseCase(repository, memberships);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdQuery(household.Id, new DomuActor(ownerUserId, DomuActorType.Zitadel)),
            CancellationToken.None);

        Assert.Equal(household.Id, result.Id);
        Assert.Equal(ownerMemberId, result.OwnerMemberId);
        Assert.Equal("Home", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHouseholdBelongsToAnotherOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var repository = new FakeHouseholdRepository(household);
        var useCase = new GetHouseholdUseCase(repository, new FakeHouseholdMembershipRepository());

        var action = () => useCase.ExecuteAsync(
            new GetHouseholdQuery(household.Id, new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel)),
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

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerMemberId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _storedHouseholds.Where(household => household.OwnerMemberId == ownerMemberId).ToArray());
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
