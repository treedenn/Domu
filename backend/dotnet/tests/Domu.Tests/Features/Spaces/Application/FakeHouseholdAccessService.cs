using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Application.Households;

namespace Domu.Tests.Features.Spaces.Application;

internal sealed class FakeHouseholdAccessService : IHouseholdAccessService
{
    private readonly Guid _memberId = Guid.NewGuid();

    public bool DenyAccess { get; set; }

    public Task EnsureCanAccessHouseholdAsync(
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }

    public Task EnsureCanAccessHouseholdAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }

    public Task<Guid> GetRequiredMemberIdAsync(DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.FromResult(_memberId);
    }

    public Task<Guid> GetRequiredMemberIdAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.FromResult(_memberId);
    }
}
