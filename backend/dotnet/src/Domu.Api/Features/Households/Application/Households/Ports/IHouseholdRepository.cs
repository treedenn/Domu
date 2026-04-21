using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Application.Households.Ports;

public interface IHouseholdRepository
{
    Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Household>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    Task AddAsync(Household household, CancellationToken cancellationToken);
    Task UpdateAsync(Household household, CancellationToken cancellationToken);
    Task DeleteAsync(Guid householdId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
