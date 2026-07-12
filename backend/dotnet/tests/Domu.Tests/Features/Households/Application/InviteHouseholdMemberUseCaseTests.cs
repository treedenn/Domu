using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

public sealed class InviteHouseholdMemberUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesPendingInvitationAndSendsIt()
    {
        var ownerId = Guid.NewGuid();
        var household = new Household(Guid.NewGuid(), ownerId, "Home");
        var householdRepository = new FakeHouseholdRepository(household);
        var membershipRepository = new FakeHouseholdMembershipRepository();
        var ownerMember = new HouseholdMember(
            Guid.NewGuid(),
            household.Id,
            ownerId,
            "Owner",
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow);
        await membershipRepository.AddMemberAsync(ownerMember, CancellationToken.None);
        var sender = new FakeHouseholdInvitationSender();
        var useCase = new InviteHouseholdMemberUseCase(householdRepository, membershipRepository, sender);

        var result = await useCase.ExecuteAsync(
            new InviteHouseholdMemberCommand(new DomuActor(ownerId, DomuActorType.Zitadel), household.Id, " Person@Example.COM ", "Alex", HouseholdMemberRole.Admin),
            CancellationToken.None);

        Assert.Equal(household.Id, result.HouseholdId);
        Assert.Equal("person@example.com", result.Email);
        Assert.Equal(HouseholdMemberRole.Admin, result.Role);
        Assert.Equal("Alex", result.DisplayName);
        Assert.Equal(ownerMember.Id, result.InvitedByMemberId);
        Assert.Equal(HouseholdInvitationStatus.Pending, result.Status);
        Assert.Single(membershipRepository.Invitations);
        Assert.Single(sender.SentInvitations);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequesterIsNotOwner_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var useCase = new InviteHouseholdMemberUseCase(
            new FakeHouseholdRepository(household),
            new FakeHouseholdMembershipRepository(),
            new FakeHouseholdInvitationSender());

        var action = () => useCase.ExecuteAsync(
            new InviteHouseholdMemberCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), household.Id, "person@example.com", "Alex", HouseholdMemberRole.Member),
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

        public Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Household>>(
                _households.Where(household => household.OwnerId == ownerId).ToArray());
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
