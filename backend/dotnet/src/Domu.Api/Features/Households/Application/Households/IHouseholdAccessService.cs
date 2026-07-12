using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public interface IHouseholdAccessService
{
    Task EnsureCanAccessHouseholdAsync(DomuActor actor, Guid householdId, CancellationToken cancellationToken);
    Task<Guid> GetRequiredMemberIdAsync(DomuActor actor, Guid householdId, CancellationToken cancellationToken);
}
