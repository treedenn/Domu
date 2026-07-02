using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class GetShoppingListsUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService)
{
    public async Task<IReadOnlyList<ShoppingListView>> ExecuteAsync(
        GetShoppingListsQuery query,
        CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(query.HouseholdId, query.UserId, cancellationToken);
        var lists = await shoppingListRepository.GetActiveByHouseholdAsync(query.HouseholdId, cancellationToken);
        return lists.Select(ShoppingListView.FromDomain).ToList();
    }
}
