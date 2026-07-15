using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.ShoppingLists.Interface.ShoppingLists;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/shopping-lists")]
[Tags("Shopping Lists")]
public sealed class ShoppingListsController(
    IActorAccessor actorAccessor,
    GetShoppingListsUseCase getShoppingListsUseCase,
    GetShoppingListUseCase getShoppingListUseCase,
    CreateShoppingListUseCase createShoppingListUseCase,
    UpdateShoppingListUseCase updateShoppingListUseCase,
    DeleteShoppingListUseCase deleteShoppingListUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ShoppingListView>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShoppingListView>>>> GetLists(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var lists = await getShoppingListsUseCase.ExecuteAsync(
                new GetShoppingListsQuery(actorAccessor.DomuActor, householdId), cancellationToken);
            return Ok(new ApiResponse<IReadOnlyList<ShoppingListView>>(lists));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{shoppingListId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ShoppingListView>>> GetList(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await getShoppingListUseCase.ExecuteAsync(
                new GetShoppingListQuery(actorAccessor.DomuActor, householdId, shoppingListId), cancellationToken);
            return Ok(new ApiResponse<ShoppingListView>(list));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListView>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ShoppingListView>>> CreateList(
        Guid householdId,
        CreateShoppingListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await createShoppingListUseCase.ExecuteAsync(
                new CreateShoppingListCommand(actorAccessor.DomuActor, householdId, request.Name), cancellationToken);
            return CreatedAtAction(nameof(GetList), new { householdId, shoppingListId = list.Id }, new ApiResponse<ShoppingListView>(list));
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
    [ProducesResponseType(typeof(ApiResponse<ShoppingListView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ShoppingListView>>> UpdateList(
        Guid householdId,
        Guid shoppingListId,
        UpdateShoppingListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await updateShoppingListUseCase.ExecuteAsync(
                new UpdateShoppingListCommand(actorAccessor.DomuActor, householdId, shoppingListId, request.Name,
                    request.Archived),
                cancellationToken);
            return Ok(new ApiResponse<ShoppingListView>(list));
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
                new DeleteShoppingListCommand(actorAccessor.DomuActor, householdId, shoppingListId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
