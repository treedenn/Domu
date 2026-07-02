using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.ShoppingLists.Interface.ShoppingLists;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/shopping-list")]
[Tags("Shopping Lists")]
public sealed class ShoppingListsController(
    IUserAccessor userAccessor,
    GetOrCreateDefaultShoppingListUseCase getOrCreateDefaultShoppingListUseCase)
    : ControllerBase
{
    [HttpGet("default")]
    [ProducesResponseType(typeof(ShoppingListView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListView>> GetDefault(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var shoppingList = await getOrCreateDefaultShoppingListUseCase.ExecuteAsync(
                new GetOrCreateDefaultShoppingListQuery(userAccessor.User.Id, householdId),
                cancellationToken);

            return Ok(shoppingList);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
