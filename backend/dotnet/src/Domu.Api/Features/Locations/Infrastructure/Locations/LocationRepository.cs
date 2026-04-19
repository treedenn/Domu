using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Features.Locations.Domain.Locations;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Locations.Infrastructure.Locations;

public sealed class LocationRepository(AppDbContext dbContext) : ILocationRepository
{
    public async Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Locations
            .SingleOrDefaultAsync(location => location.Id == locationId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        await dbContext.Locations.AddAsync(LocationEntity.FromDomain(location), cancellationToken);
    }

    public async Task UpdateAsync(Location location, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Locations
            .SingleOrDefaultAsync(existingLocation => existingLocation.Id == location.Id, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Location '{location.Id}' was not found.");

        existingEntity.UpdateFromDomain(location);
    }

    public async Task DeleteAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Locations
            .SingleOrDefaultAsync(location => location.Id == locationId, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Location '{locationId}' was not found.");

        dbContext.Locations.Remove(existingEntity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
