using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class GetHouseholdInvitationsUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<IReadOnlyList<HouseholdInvitationView>> ExecuteAsync(
        GetHouseholdInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var household = await householdRepository.GetByIdAsync(query.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        if (household.OwnerId != query.Actor.ActorId
            && !await membershipRepository.IsMemberAsync(query.HouseholdId, query.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        var invitations = await membershipRepository.GetPendingInvitationsAsync(
            query.HouseholdId,
            cancellationToken);

        return invitations.Select(HouseholdInvitationView.FromDomain).ToArray();
    }
}
