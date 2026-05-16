using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public interface IGetOrCreateDefaultShoppingListUseCase
{
    Task<ShoppingListView> ExecuteAsync(GetOrCreateDefaultShoppingListQuery query, CancellationToken cancellationToken);
}
