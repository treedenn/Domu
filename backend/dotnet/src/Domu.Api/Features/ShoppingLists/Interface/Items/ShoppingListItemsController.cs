using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.ShoppingLists.Application.Items;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Queries;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.ShoppingLists.Interface.Items;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/shopping-lists/{shoppingListId:guid}/items")]
[Tags("Shopping List Items")]
public sealed class ShoppingListItemsController(
    IActorAccessor actorAccessor,
    GetShoppingListItemsUseCase getShoppingListItemsUseCase,
    CreateShoppingListItemUseCase createShoppingListItemUseCase,
    UpdateShoppingListItemUseCase updateShoppingListItemUseCase,
    SetShoppingListItemCheckedStateUseCase setShoppingListItemCheckedStateUseCase,
    DeleteShoppingListItemUseCase deleteShoppingListItemUseCase,
    ClearCheckedShoppingListItemsUseCase clearCheckedShoppingListItemsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ShoppingListItemView>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShoppingListItemView>>>> GetItems(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await getShoppingListItemsUseCase.ExecuteAsync(
                new GetShoppingListItemsQuery(actorAccessor.DomuActor, householdId, shoppingListId),
                cancellationToken);

            return Ok(new ApiResponse<IReadOnlyList<ShoppingListItemView>>(items));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListItemView>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ShoppingListItemView>>> CreateItem(
        Guid householdId,
        Guid shoppingListId,
        CreateShoppingListItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await createShoppingListItemUseCase.ExecuteAsync(
                new CreateShoppingListItemCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    shoppingListId,
                    request.Name,
                    request.Quantity,
                    request.ContainerQuantity,
                    request.ContainerUnit,
                    request.Note,
                    request.SpaceId,
                    request.ItemId),
                cancellationToken);

            return CreatedAtAction(nameof(GetItems), new { householdId, shoppingListId }, new ApiResponse<ShoppingListItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid shopping list item.", Detail = exception.Message });
        }
    }

    [HttpPatch("{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ShoppingListItemView>>> UpdateItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        UpdateShoppingListItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await updateShoppingListItemUseCase.ExecuteAsync(
                new UpdateShoppingListItemCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    shoppingListId,
                    itemId,
                    request.Name,
                    request.Quantity,
                    request.ContainerQuantity,
                    request.ContainerUnit,
                    request.Note,
                    request.SpaceId,
                    request.ItemId,
                    request.SortOrder),
                cancellationToken);

            return Ok(new ApiResponse<ShoppingListItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid shopping list item.", Detail = exception.Message });
        }
    }

    [HttpPost("{itemId:guid}/check")]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ApiResponse<ShoppingListItemView>>> CheckItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return UpdateCheckedStateAsync(
            householdId,
            shoppingListId,
            itemId,
            true,
            cancellationToken);
    }

    [HttpPost("{itemId:guid}/uncheck")]
    [ProducesResponseType(typeof(ApiResponse<ShoppingListItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ApiResponse<ShoppingListItemView>>> UncheckItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return UpdateCheckedStateAsync(
            householdId,
            shoppingListId,
            itemId,
            false,
            cancellationToken);
    }

    [HttpDelete("{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            await deleteShoppingListItemUseCase.ExecuteAsync(
                new DeleteShoppingListItemCommand(actorAccessor.DomuActor, householdId, shoppingListId, itemId),
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("checked")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCheckedItems(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            await clearCheckedShoppingListItemsUseCase.ExecuteAsync(
                new ClearCheckedShoppingListItemsCommand(actorAccessor.DomuActor, householdId, shoppingListId),
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<ActionResult<ApiResponse<ShoppingListItemView>>> UpdateCheckedStateAsync(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        bool isChecked,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await setShoppingListItemCheckedStateUseCase.ExecuteAsync(
                new SetShoppingListItemCheckedStateCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    shoppingListId,
                    itemId,
                    isChecked),
                cancellationToken);

            return Ok(new ApiResponse<ShoppingListItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
