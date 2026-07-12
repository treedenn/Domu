using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

internal sealed class FakeSpaceAccessService : ISpaceAccessService
{
    public bool DenyAccess { get; set; }

    public Task EnsureCanAccessSpaceAsync(
        Guid householdId,
        Guid spaceId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }

    public Task EnsureSpaceBelongsToHouseholdAsync(
        Guid spaceId,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }

    public Task EnsureCanAccessSpaceTargetAsync(
        Guid householdId,
        Guid? parentId,
        DomuActor actor,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }
}
