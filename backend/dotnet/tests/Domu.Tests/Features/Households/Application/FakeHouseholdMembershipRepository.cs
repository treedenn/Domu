using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

internal sealed class FakeHouseholdMembershipRepository : IHouseholdMembershipRepository
{
    private readonly List<HouseholdMember> _members = [];
    private readonly List<HouseholdInvitation> _invitations = [];

    public IReadOnlyList<HouseholdMember> Members => _members;
    public IReadOnlyList<HouseholdInvitation> Invitations => _invitations;

    public Task<bool> IsMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_members.Any(member => member.HouseholdId == householdId && member.UserId == userId));
    }

    public Task<HouseholdMember?> GetMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_members.SingleOrDefault(member =>
            member.HouseholdId == householdId && member.UserId == userId));
    }

    public Task<HouseholdMember?> GetMemberByIdAsync(
        Guid householdId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_members.SingleOrDefault(member =>
            member.HouseholdId == householdId && member.Id == memberId));
    }

    public Task<IReadOnlyList<HouseholdMember>> GetMembersAsync(Guid householdId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<HouseholdMember>>(
            _members.Where(member => member.HouseholdId == householdId).ToArray());
    }

    public Task AddMemberAsync(HouseholdMember member, CancellationToken cancellationToken)
    {
        _members.Add(member);
        return Task.CompletedTask;
    }

    public Task UpdateMemberAsync(HouseholdMember member, CancellationToken cancellationToken)
    {
        var index = _members.FindIndex(existingMember => existingMember.Id == member.Id);
        if (index >= 0)
            _members[index] = member;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<HouseholdInvitation>> GetPendingInvitationsAsync(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<HouseholdInvitation>>(
            _invitations
                .Where(invitation => invitation.HouseholdId == householdId
                                     && invitation.Status == HouseholdInvitationStatus.Pending
                                     && invitation.ExpiresAt > DateTimeOffset.UtcNow)
                .ToArray());
    }

    public Task<HouseholdInvitation?> GetPendingInvitationByEmailAsync(
        Guid householdId,
        string email,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_invitations.SingleOrDefault(invitation =>
            invitation.HouseholdId == householdId
            && invitation.Email == email
            && invitation.Status == HouseholdInvitationStatus.Pending
            && invitation.ExpiresAt > DateTimeOffset.UtcNow));
    }

    public Task<HouseholdInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return Task.FromResult(_invitations.SingleOrDefault(invitation => invitation.Token == token));
    }

    public Task AddInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        _invitations.Add(invitation);
        return Task.CompletedTask;
    }

    public Task UpdateInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        var index = _invitations.FindIndex(existingInvitation => existingInvitation.Id == invitation.Id);
        if (index >= 0)
            _invitations[index] = invitation;

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
