namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface ISpaceAccessService
{
    Task EnsureCanAccessSpaceAsync(
        Guid householdId,
        Guid spaceId,
        Guid userId,
        CancellationToken cancellationToken);

    Task EnsureSpaceBelongsToHouseholdAsync(
        Guid spaceId,
        Guid householdId,
        CancellationToken cancellationToken);

    Task EnsureCanAccessSpaceTargetAsync(
        Guid householdId,
        Guid? parentId,
        Guid userId,
        CancellationToken cancellationToken);
}
