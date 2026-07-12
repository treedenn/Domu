using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface ISpaceAccessService
{
    Task EnsureCanAccessSpaceAsync(
        Guid householdId,
        Guid spaceId,
        DomuActor actor,
        CancellationToken cancellationToken);

    Task EnsureSpaceBelongsToHouseholdAsync(
        Guid spaceId,
        Guid householdId,
        CancellationToken cancellationToken);

    Task EnsureCanAccessSpaceTargetAsync(
        Guid householdId,
        Guid? parentId,
        DomuActor actor,
        CancellationToken cancellationToken);
}