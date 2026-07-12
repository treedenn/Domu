using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class SpaceAccessService(
    IHouseholdAccessService householdAccessService,
    ISpaceRepository spaceRepository)
    : ISpaceAccessService
{
    public async Task EnsureCanAccessSpaceAsync(
        Guid householdId,
        Guid spaceId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(actor, householdId, cancellationToken);
        await EnsureSpaceBelongsToHouseholdAsync(spaceId, householdId, cancellationToken);
    }

    public async Task EnsureCanAccessSpaceTargetAsync(
        Guid householdId,
        Guid? parentId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(actor, householdId, cancellationToken);

        if (parentId is not null)
            await EnsureSpaceBelongsToHouseholdAsync(parentId.Value, householdId, cancellationToken);
    }

    public async Task EnsureSpaceBelongsToHouseholdAsync(
        Guid spaceId,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var space = await spaceRepository.GetByIdAsync(spaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{spaceId}' was not found.");

        if (space.HouseholdId != householdId)
            throw new KeyNotFoundException($"Space '{spaceId}' was not found.");
    }
}
