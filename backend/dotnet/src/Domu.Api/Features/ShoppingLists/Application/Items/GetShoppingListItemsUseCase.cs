using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class GetShoppingListItemsUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService)
    : IGetShoppingListItemsUseCase
{
    public async Task<IReadOnlyList<ShoppingListItemView>> ExecuteAsync(
        GetShoppingListItemsQuery query,
        CancellationToken cancellationToken)
    {
        await ShoppingListItemUseCaseGuards.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            query.HouseholdId,
            query.ShoppingListId,
            query.UserId,
            cancellationToken);

        var items = await shoppingListItemRepository.GetItemsAsync(query.ShoppingListId, cancellationToken);

        return items.Select(ShoppingListItemView.FromDomain).ToArray();
    }
}
