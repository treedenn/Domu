using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class GetHouseholdMemberUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<HouseholdMemberView> ExecuteAsync(
        GetHouseholdMemberQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        _ = await householdRepository.GetByIdAsync(query.HouseholdId, cancellationToken)
            ?? throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        if (!await membershipRepository.IsMemberAsync(query.HouseholdId, query.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{query.HouseholdId}' was not found.");

        var member = await membershipRepository.GetMemberByIdAsync(query.HouseholdId, query.MemberId, cancellationToken);

        return HouseholdMemberView.FromDomain(member);
    }
}