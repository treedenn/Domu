using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Households.Infrastructure.Households;

public sealed class HouseholdRepository(AppDbContext dbContext) : IHouseholdRepository
{
    public async Task<Household?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Households
            .AsNoTracking()
            .SingleOrDefaultAsync(household => household.Id == householdId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Household>> GetAccessibleByUserIdAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        var memberHouseholdIds = dbContext.HouseholdMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Select(member => member.HouseholdId);

        var entities = await dbContext.Households
            .AsNoTracking()
            .Where(household => memberHouseholdIds.Contains(household.Id))
            .OrderBy(household => household.Name)
            .ThenBy(household => household.Id)
            .ToArrayAsync(cancellationToken);

        return entities.Select(household => household.ToDomain()).ToArray();
    }

    public async Task AddAsync(Household household, CancellationToken cancellationToken)
    {
        await dbContext.Households.AddAsync(HouseholdEntity.FromDomain(household), cancellationToken);
    }

    public async Task UpdateAsync(Household household, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Households
            .SingleOrDefaultAsync(existingHousehold => existingHousehold.Id == household.Id, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Household '{household.Id}' was not found.");

        existingEntity.UpdateFromDomain(household);
    }

    public async Task DeleteAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.Households
            .SingleOrDefaultAsync(household => household.Id == householdId, cancellationToken);

        if (existingEntity is null)
            throw new KeyNotFoundException($"Household '{householdId}' was not found.");

        dbContext.Households.Remove(existingEntity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}