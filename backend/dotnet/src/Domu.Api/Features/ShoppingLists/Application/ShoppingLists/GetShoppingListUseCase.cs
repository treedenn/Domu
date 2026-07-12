using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class GetShoppingListUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService)
{
    public async Task<ShoppingListView> ExecuteAsync(GetShoppingListQuery query, CancellationToken cancellationToken)
    {
        var list = await ShoppingListPermissionPolicy.GetAccessibleListAsync(
            shoppingListRepository, householdAccessService, query.HouseholdId, query.ShoppingListId, query.Actor,
            cancellationToken);
        return ShoppingListView.FromDomain(list);
    }
}