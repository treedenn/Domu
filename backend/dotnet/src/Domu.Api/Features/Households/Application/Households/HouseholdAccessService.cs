using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

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
        _ = await GetAccessibleMemberAsync(householdId, userId, cancellationToken);
    }

    public async Task<Guid> GetRequiredMemberIdAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await GetAccessibleMemberAsync(householdId, userId, cancellationToken);
        if (member is null)
            throw new InvalidOperationException(
                $"Household '{householdId}' is accessible to user '{userId}' but has no linked household member.");

        return member.Id;
    }

    private async Task<HouseholdMember?> GetAccessibleMemberAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        var member = await membershipRepository.GetMemberAsync(householdId, userId, cancellationToken);
        if (household.OwnerId != userId && member is null)
            throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        return member;
    }
}
