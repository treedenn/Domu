namespace Domu.Api.Features.Households.Application.Households;

public interface IHouseholdAccessService
{
    Task EnsureCanAccessHouseholdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
    Task<Guid> GetRequiredMemberIdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken);
}
