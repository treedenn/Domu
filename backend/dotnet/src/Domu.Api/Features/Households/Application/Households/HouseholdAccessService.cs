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
        if (member is null)
            throw new InvalidOperationException(
                $"Household '{householdId}' is accessible to actor '{actor.ActorId}' but has no linked household member.");

        return member.Id;
    }

    private async Task<HouseholdMember?> GetAccessibleMemberAsync(
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        var member = await membershipRepository.GetMemberAsync(householdId, actor.ActorId, cancellationToken);
        if (household.OwnerId != actor.ActorId && member is null)
            throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        return member;
    }
}
