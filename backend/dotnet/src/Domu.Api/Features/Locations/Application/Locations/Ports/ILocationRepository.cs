using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Api.Features.Locations.Application.Locations.Ports;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken);
    Task AddAsync(Location location, CancellationToken cancellationToken);
    Task UpdateAsync(Location location, CancellationToken cancellationToken);
    Task DeleteAsync(Guid locationId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
