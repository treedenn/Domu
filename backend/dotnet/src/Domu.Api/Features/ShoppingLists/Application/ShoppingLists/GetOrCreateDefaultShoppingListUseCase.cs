using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class GetOrCreateDefaultShoppingListUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService)
    : IGetOrCreateDefaultShoppingListUseCase
{
    public async Task<ShoppingListView> ExecuteAsync(
        GetOrCreateDefaultShoppingListQuery query,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(query.HouseholdId, query.UserId, cancellationToken);

        var existing = await shoppingListRepository.GetActiveDefaultByHouseholdAsync(
            query.HouseholdId,
            cancellationToken);
        if (existing is not null)
            return ShoppingListView.FromDomain(existing);

        var now = DateTimeOffset.UtcNow;
        var shoppingList = new ShoppingList(
            Guid.CreateVersion7(),
            query.HouseholdId,
            "Shopping list",
            isDefault: true,
            query.UserId,
            now,
            now);

        await shoppingListRepository.AddAsync(shoppingList, cancellationToken);
        await shoppingListRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListView.FromDomain(shoppingList);
    }
}
