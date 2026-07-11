using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Ports;

public interface IHouseholdMembershipRepository
{
    Task<bool> IsMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
    Task<HouseholdMember?> GetMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
    Task<HouseholdMember?> GetMemberByIdAsync(Guid householdId, Guid memberId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HouseholdMember>> GetMembersAsync(Guid householdId, CancellationToken cancellationToken);
    Task AddMemberAsync(HouseholdMember member, CancellationToken cancellationToken);
    Task UpdateMemberAsync(HouseholdMember member, CancellationToken cancellationToken);
    Task<IReadOnlyList<HouseholdInvitation>> GetPendingInvitationsAsync(Guid householdId, CancellationToken cancellationToken);
    Task<HouseholdInvitation?> GetPendingInvitationByEmailAsync(Guid householdId, string email, CancellationToken cancellationToken);
    Task<HouseholdInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken);
    Task AddInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken);
    Task UpdateInvitationAsync(HouseholdInvitation invitation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
