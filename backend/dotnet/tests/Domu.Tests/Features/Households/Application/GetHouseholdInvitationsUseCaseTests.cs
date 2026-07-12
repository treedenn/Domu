using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class GetHouseholdInvitationsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsPendingInvitationsForAccessibleHousehold()
    {
        var ownerMemberId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerMemberId, "Home");
        var householdRepository = new FakeHouseholdRepository(household);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var now = DateTimeOffset.UtcNow;
        await membershipRepository.AddMemberAsync(
            new HouseholdMember(ownerMemberId, household.Id, ownerMemberId, "Owner", HouseholdMemberRole.Owner, now),
            CancellationToken.None);
        await membershipRepository.AddInvitationAsync(
            new HouseholdInvitation(
                Guid.NewGuid(),
                household.Id,
                "person@example.com",
                "Alex",
                ownerMemberId,
                HouseholdMemberRole.Member,
                "token",
                now,
                now.AddDays(1)),
            CancellationToken.None);
        var useCase = new GetHouseholdInvitationsUseCase(householdRepository, membershipRepository);

        var result = await useCase.ExecuteAsync(
            new GetHouseholdInvitationsQuery(household.Id, new DomuActor(ownerMemberId, DomuActorType.Zitadel)),
            CancellationToken.None);

        var invitation = Assert.Single(result);
        Assert.Equal("person@example.com", invitation.Email);
        Assert.Equal(HouseholdInvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserCannotAccessHousehold_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var useCase = new GetHouseholdInvitationsUseCase(
            new FakeHouseholdRepository(household),
            new FakeHouseholdMembershipRepository());

        var action = () => useCase.ExecuteAsync(
            new GetHouseholdInvitationsQuery(household.Id, new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel)),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeHouseholdRepository(params Household[] seededHouseholds) : IHouseholdRepository
    {
        private readonly List<Household> _households = seededHouseholds.ToList();

        public Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_households.SingleOrDefault(household => household.Id == householdId));
        }

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerMemberId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _households.Where(household => household.OwnerMemberId == ownerMemberId).ToArray());
        }

        public Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return GetByOwnerIdAsync(userId, cancellationToken);
        }

        public Task AddAsync(Household household, CancellationToken cancellationToken)
        {
            _households.Add(household);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Household household, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
        {
            _households.RemoveAll(household => household.Id == householdId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
