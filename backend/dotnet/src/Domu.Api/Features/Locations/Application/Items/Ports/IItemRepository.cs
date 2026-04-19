using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Application.Items.Ports;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Item>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken);
    Task AddAsync(Item item, CancellationToken cancellationToken);
    Task UpdateAsync(Item item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid itemId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
