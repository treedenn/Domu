using Domu.Api.Features.Households.Application.Households;

namespace Domu.Tests.Features.Spaces.Application;

internal sealed class FakeHouseholdAccessService : IHouseholdAccessService
{
    public bool DenyAccess { get; set; }

    public Task EnsureCanAccessHouseholdAsync(
        Guid householdId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (DenyAccess)
            throw new KeyNotFoundException();

        return Task.CompletedTask;
    }
}
