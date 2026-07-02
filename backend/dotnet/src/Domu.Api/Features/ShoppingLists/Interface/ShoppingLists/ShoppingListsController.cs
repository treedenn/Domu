using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.ShoppingLists.Interface.ShoppingLists;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/shopping-lists")]
[Tags("Shopping Lists")]
public sealed class ShoppingListsController(
    IUserAccessor userAccessor,
    GetShoppingListsUseCase getShoppingListsUseCase,
    GetShoppingListUseCase getShoppingListUseCase,
    CreateShoppingListUseCase createShoppingListUseCase,
    UpdateShoppingListUseCase updateShoppingListUseCase,
    DeleteShoppingListUseCase deleteShoppingListUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShoppingListView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ShoppingListView>>> GetLists(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var lists = await getShoppingListsUseCase.ExecuteAsync(
                new GetShoppingListsQuery(userAccessor.User.Id, householdId), cancellationToken);
            return Ok(lists);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{shoppingListId:guid}")]
    [ProducesResponseType(typeof(ShoppingListView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListView>> GetList(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await getShoppingListUseCase.ExecuteAsync(
                new GetShoppingListQuery(userAccessor.User.Id, householdId, shoppingListId), cancellationToken);
            return Ok(list);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingListView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListView>> CreateList(
        Guid householdId,
        CreateShoppingListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await createShoppingListUseCase.ExecuteAsync(
                new CreateShoppingListCommand(userAccessor.User.Id, householdId, request.Name), cancellationToken);
            return CreatedAtAction(nameof(GetList), new { householdId, shoppingListId = list.Id }, list);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid shopping list.", Detail = exception.Message });
        }
    }

    [HttpPut("{shoppingListId:guid}")]
    [ProducesResponseType(typeof(ShoppingListView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListView>> UpdateList(
        Guid householdId,
        Guid shoppingListId,
        UpdateShoppingListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await updateShoppingListUseCase.ExecuteAsync(
                new UpdateShoppingListCommand(userAccessor.User.Id, householdId, shoppingListId, request.Name),
                cancellationToken);
            return Ok(list);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid shopping list.", Detail = exception.Message });
        }
    }

    [HttpDelete("{shoppingListId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteList(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            await deleteShoppingListUseCase.ExecuteAsync(
                new DeleteShoppingListCommand(userAccessor.User.Id, householdId, shoppingListId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
