using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class GetHouseholdMembersUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<IReadOnlyList<HouseholdMemberView>> ExecuteAsync(
        GetHouseholdMembersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var household = await householdRepository.GetByIdAsync(query.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        if (!await membershipRepository.IsMemberAsync(query.HouseholdId, query.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        var members = await membershipRepository.GetMembersAsync(query.HouseholdId, cancellationToken);

        return members.Select(HouseholdMemberView.FromDomain).ToArray();
    }
}