using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Ports;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Item>> GetBySpaceAsync(Guid spaceId, CancellationToken cancellationToken);
    Task AddAsync(Item item, CancellationToken cancellationToken);
    Task UpdateAsync(Item item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid itemId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
