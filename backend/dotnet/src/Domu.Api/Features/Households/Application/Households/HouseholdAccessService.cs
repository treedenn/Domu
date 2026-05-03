using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class HouseholdAccessService(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
    : IHouseholdAccessService
{
    public async Task EnsureCanAccessHouseholdAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        if (household.OwnerId != userId
            && !await membershipRepository.IsMemberAsync(householdId, userId, cancellationToken))
            throw new KeyNotFoundException($"Household '{householdId}' was not found.");
    }
}
