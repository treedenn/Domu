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

    public async Task<bool> IsDescendantAsync(
        Guid ancestorSpaceId,
        Guid candidateDescendantId,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        Guid? currentSpaceId = candidateDescendantId;
        var visitedSpaceIds = new HashSet<Guid>();

        while (currentSpaceId is not null && visitedSpaceIds.Add(currentSpaceId.Value))
        {
            var currentSpace = await dbContext.Spaces
                .AsNoTracking()
                .Where(space => space.Id == currentSpaceId.Value && space.HouseholdId == householdId)
                .Select(space => new { space.Id, space.ParentId })
                .SingleOrDefaultAsync(cancellationToken);

            if (currentSpace is null)
                return false;
            if (currentSpace.Id == ancestorSpaceId)
                return true;

            currentSpaceId = currentSpace.ParentId;
        }

        return false;
    }

    public async Task<bool> HasChildrenOrItemsAsync(Guid spaceId, CancellationToken cancellationToken)
    {
        if (await dbContext.Spaces.AnyAsync(space => space.ParentId == spaceId, cancellationToken))
            return true;

        return await dbContext.Items.AnyAsync(item => item.SpaceId == spaceId, cancellationToken);
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
