using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Spaces.Infrastructure.Spaces;

public sealed class SpaceRepository(AppDbContext dbContext) : ISpaceRepository
{
    public async Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Spaces
            .SingleOrDefaultAsync(existingSpace => existingSpace.Id == spaceId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddAsync(Space space, CancellationToken cancellationToken)
    {
        await dbContext.Spaces.AddAsync(SpaceEntity.FromDomain(space), cancellationToken);
    }

    public async Task UpdateAsync(Space space, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Spaces
            .SingleOrDefaultAsync(existingSpace => existingSpace.Id == space.Id, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Space '{space.Id}' was not found.");

        existingEntity.UpdateFromDomain(space);
    }

    public async Task DeleteAsync(Guid spaceId, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Spaces
            .SingleOrDefaultAsync(existingSpace => existingSpace.Id == spaceId, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Space '{spaceId}' was not found.");

        dbContext.Spaces.Remove(existingEntity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}