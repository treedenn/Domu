using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public interface IHouseholdAccessService
{
    Task EnsureCanAccessHouseholdAsync(DomuActor actor, Guid householdId, CancellationToken cancellationToken);
    Task<Guid> GetRequiredMemberIdAsync(DomuActor actor, Guid householdId, CancellationToken cancellationToken);
    Task EnsureCanAccessHouseholdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
    Task<Guid> GetRequiredMemberIdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
}
