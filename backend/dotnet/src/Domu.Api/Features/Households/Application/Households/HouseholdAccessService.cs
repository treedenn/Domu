using Domu.Api.Features.Auth.Domain;
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
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        _ = await GetAccessibleMemberAsync(actor, householdId, cancellationToken);
    }

    public async Task<Guid> GetRequiredMemberIdAsync(
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var member = await GetAccessibleMemberAsync(actor, householdId, cancellationToken);
        return member.Id;
    }

    private async Task<HouseholdMember> GetAccessibleMemberAsync(
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        return await membershipRepository.GetMemberAsync(householdId, actor.ActorId, cancellationToken)
               ?? throw new KeyNotFoundException($"Household '{householdId}' was not found.");
    }
}