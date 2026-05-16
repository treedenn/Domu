using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.ShoppingLists;

public sealed class ShoppingListRepository(AppDbContext dbContext) : IShoppingListRepository
{
    public async Task<ShoppingList?> GetActiveDefaultByHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShoppingLists
            .AsNoTracking()
            .SingleOrDefaultAsync(
                shoppingList => shoppingList.HouseholdId == householdId
                                && shoppingList.IsDefault
                                && shoppingList.ArchivedAt == null,
                cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<ShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShoppingLists
            .AsNoTracking()
            .SingleOrDefaultAsync(shoppingList => shoppingList.Id == shoppingListId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddAsync(ShoppingList shoppingList, CancellationToken cancellationToken)
    {
        await dbContext.ShoppingLists.AddAsync(ShoppingListEntity.FromDomain(shoppingList), cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
