using Domu.Api.Features.ShoppingLists.Application.Items;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.ShoppingLists.Interface.Items;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/shopping-lists/{shoppingListId:guid}/items")]
[Tags("Shopping List Items")]
public sealed class ShoppingListItemsController(
    IUserAccessor userAccessor,
    IGetShoppingListItemsUseCase getShoppingListItemsUseCase,
    ICreateShoppingListItemUseCase createShoppingListItemUseCase,
    IUpdateShoppingListItemUseCase updateShoppingListItemUseCase,
    ICheckShoppingListItemUseCase checkShoppingListItemUseCase,
    IUncheckShoppingListItemUseCase uncheckShoppingListItemUseCase,
    IDeleteShoppingListItemUseCase deleteShoppingListItemUseCase,
    IClearCheckedShoppingListItemsUseCase clearCheckedShoppingListItemsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShoppingListItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ShoppingListItemView>>> GetItems(
        Guid householdId,
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await getShoppingListItemsUseCase.ExecuteAsync(
                new GetShoppingListItemsQuery(userAccessor.User.Id, householdId, shoppingListId),
                cancellationToken);

            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingListItemView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListItemView>> CreateItem(
        Guid householdId,
        Guid shoppingListId,
        CreateShoppingListItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await createShoppingListItemUseCase.ExecuteAsync(
                new CreateShoppingListItemCommand(
                    userAccessor.User.Id,
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

            return CreatedAtAction(nameof(GetItems), new { householdId, shoppingListId }, item);
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
    [ProducesResponseType(typeof(ShoppingListItemView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListItemView>> UpdateItem(
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
                    userAccessor.User.Id,
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

            return Ok(item);
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
    [ProducesResponseType(typeof(ShoppingListItemView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ShoppingListItemView>> CheckItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return UpdateCheckedStateAsync(
            checkShoppingListItemUseCase,
            householdId,
            shoppingListId,
            itemId,
            cancellationToken);
    }

    [HttpPost("{itemId:guid}/uncheck")]
    [ProducesResponseType(typeof(ShoppingListItemView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ShoppingListItemView>> UncheckItem(
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return UpdateCheckedStateAsync(
            uncheckShoppingListItemUseCase,
            householdId,
            shoppingListId,
            itemId,
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
                new ShoppingListItemCommand(userAccessor.User.Id, householdId, shoppingListId, itemId),
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
                new ClearCheckedShoppingListItemsCommand(userAccessor.User.Id, householdId, shoppingListId),
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<ActionResult<ShoppingListItemView>> UpdateCheckedStateAsync(
        object useCase,
        Guid householdId,
        Guid shoppingListId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ShoppingListItemCommand(userAccessor.User.Id, householdId, shoppingListId, itemId);
            var item = useCase switch
            {
                ICheckShoppingListItemUseCase checkUseCase => await checkUseCase.ExecuteAsync(command, cancellationToken),
                IUncheckShoppingListItemUseCase uncheckUseCase => await uncheckUseCase.ExecuteAsync(command, cancellationToken),
                _ => throw new InvalidOperationException("Unsupported checked-state use case.")
            };

            return Ok(item);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
