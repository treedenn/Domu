using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class GetHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<HouseholdView> ExecuteAsync(GetHouseholdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var household = await householdRepository.GetByIdAsync(query.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        if (!await membershipRepository.IsMemberAsync(query.HouseholdId, query.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        return HouseholdView.FromDomain(household);
    }
}