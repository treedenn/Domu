using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class GetHouseholdMembersUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<HouseholdMembersView> ExecuteAsync(
        GetHouseholdMembersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var household = await householdRepository.GetByIdAsync(query.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        if (!await membershipRepository.IsMemberAsync(query.HouseholdId, query.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        var members = await membershipRepository.GetMembersAsync(query.HouseholdId, cancellationToken);
        var canManageMembers = await membershipRepository.IsOwnerAsync(
            household,
            query.Actor.ActorId,
            cancellationToken);

        return new HouseholdMembersView(
            members
                .Where(member => !member.Archived)
                .Select(HouseholdMemberView.FromDomain)
                .ToArray(),
            canManageMembers);
    }
}
