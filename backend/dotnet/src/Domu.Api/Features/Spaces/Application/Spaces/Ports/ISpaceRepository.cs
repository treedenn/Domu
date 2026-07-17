using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Application.Spaces.Ports;

public interface ISpaceRepository
{
    Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken);
    Task<bool> IsDescendantAsync(
        Guid ancestorSpaceId,
        Guid candidateDescendantId,
        Guid householdId,
        CancellationToken cancellationToken);
    Task<bool> HasChildrenOrItemsAsync(Guid spaceId, CancellationToken cancellationToken);
    Task AddAsync(Space space, CancellationToken cancellationToken);
    Task UpdateAsync(Space space, CancellationToken cancellationToken);
    Task DeleteAsync(Guid spaceId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
