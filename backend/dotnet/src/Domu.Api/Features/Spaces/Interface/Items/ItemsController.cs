using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Items;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/spaces/{spaceId:guid}/items")]
[Tags("Items")]
public sealed class ItemsController(
    IActorAccessor actorAccessor,
    ICreateItemUseCase createItemUseCase,
    IGetSpaceItemsUseCase getSpaceItemsUseCase,
    IUpdateItemUseCase updateItemUseCase,
    IReplaceItemEntriesUseCase replaceItemEntriesUseCase,
    IDeleteItemUseCase deleteItemUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ItemView>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ItemView>>>> GetItems(
        Guid householdId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await getSpaceItemsUseCase.ExecuteAsync(
                new GetSpaceItemsQuery(actorAccessor.DomuActor, householdId, spaceId),
                cancellationToken);
            return Ok(new ApiResponse<IReadOnlyList<ItemView>>(items));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemView>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemView>>> CreateItem(
        Guid householdId,
        Guid spaceId,
        CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await createItemUseCase.ExecuteAsync(
                new CreateItemCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    spaceId,
                    request.Name,
                    request.Category,
                    request.Barcode,
                    request.Entries?.Select(entry => entry.ToDraft()).ToArray()),
                cancellationToken);

            return CreatedAtAction(nameof(GetItems), new { householdId, spaceId }, new ApiResponse<ItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid item.", Detail = exception.Message });
        }
    }

    [HttpPut("{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemView>>> UpdateItem(
        Guid householdId,
        Guid spaceId,
        Guid itemId,
        UpdateItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await updateItemUseCase.ExecuteAsync(
                new UpdateItemCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    spaceId,
                    itemId,
                    request.Name,
                    request.Category,
                    request.Barcode),
                cancellationToken);

            return Ok(new ApiResponse<ItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid item.", Detail = exception.Message });
        }
    }

    [HttpPut("{itemId:guid}/entries")]
    [ProducesResponseType(typeof(ApiResponse<ItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemView>>> ReplaceItemEntries(
        Guid householdId,
        Guid spaceId,
        Guid itemId,
        ReplaceItemEntriesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await replaceItemEntriesUseCase.ExecuteAsync(
                new ReplaceItemEntriesCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    spaceId,
                    itemId,
                    request.Entries.Select(entry => entry.ToDraft()).ToArray()),
                cancellationToken);

            return Ok(new ApiResponse<ItemView>(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid item entries.", Detail = exception.Message });
        }
    }

    [HttpDelete("{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        Guid householdId,
        Guid spaceId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            await deleteItemUseCase.ExecuteAsync(
                new DeleteItemCommand(actorAccessor.DomuActor, householdId, spaceId, itemId),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
