using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class HouseholdMembershipRepository(AppDbContext dbContext) : IHouseholdMembershipRepository
{
    public Task<bool> IsMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.HouseholdMembers
            .AsNoTracking()
            .AnyAsync(member => member.HouseholdId == householdId && member.UserId == userId, cancellationToken);
    }

    public async Task<HouseholdMember?> GetMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await dbContext.HouseholdMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                member => member.HouseholdId == householdId && member.UserId == userId,
                cancellationToken);

        return member?.ToDomain();
    }

    public async Task<IReadOnlyList<HouseholdMember>> GetMembersAsync(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var members = await dbContext.HouseholdMembers
            .AsNoTracking()
            .Where(member => member.HouseholdId == householdId)
            .OrderBy(member => member.JoinedAt)
            .ThenBy(member => member.Id)
            .ToArrayAsync(cancellationToken);

        return members.Select(member => member.ToDomain()).ToArray();
    }

    public async Task AddMemberAsync(HouseholdMember member, CancellationToken cancellationToken)
    {
        await dbContext.HouseholdMembers.AddAsync(HouseholdMemberEntity.FromDomain(member), cancellationToken);
    }

    public async Task<IReadOnlyList<HouseholdInvitation>> GetPendingInvitationsAsync(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var invitations = await dbContext.HouseholdInvitations
            .AsNoTracking()
            .Where(invitation => invitation.HouseholdId == householdId
                                 && invitation.Status == HouseholdInvitationStatus.Pending
                                 && invitation.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderBy(invitation => invitation.CreatedAt)
            .ThenBy(invitation => invitation.Id)
            .ToArrayAsync(cancellationToken);

        return invitations.Select(invitation => invitation.ToDomain()).ToArray();
    }

    public async Task<HouseholdInvitation?> GetPendingInvitationByEmailAsync(
        Guid householdId,
        string email,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.HouseholdInvitations
            .AsNoTracking()
            .Where(invitation => invitation.HouseholdId == householdId
                                 && invitation.Email == email
                                 && invitation.Status == HouseholdInvitationStatus.Pending
                                 && invitation.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return invitation?.ToDomain();
    }

    public async Task<HouseholdInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await dbContext.HouseholdInvitations
            .AsNoTracking()
            .SingleOrDefaultAsync(invitation => invitation.Token == token, cancellationToken);

        return invitation?.ToDomain();
    }

    public async Task AddInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        await dbContext.HouseholdInvitations.AddAsync(HouseholdInvitationEntity.FromDomain(invitation), cancellationToken);
    }

    public async Task UpdateInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.HouseholdInvitations
            .SingleOrDefaultAsync(existingInvitation => existingInvitation.Id == invitation.Id, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Household invitation '{invitation.Id}' was not found.");

        existingEntity.UpdateFromDomain(invitation);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
