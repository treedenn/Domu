using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Users.Interface.Auth;
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
    [ProducesResponseType(typeof(IReadOnlyList<ItemView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ItemView>>> GetItems(
        Guid householdId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await getSpaceItemsUseCase.ExecuteAsync(
                new GetSpaceItemsQuery(actorAccessor.DomuActor.ActorId, householdId, spaceId),
                cancellationToken);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemView>> CreateItem(
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

            return CreatedAtAction(nameof(GetItems), new { householdId, spaceId }, item);
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
    [ProducesResponseType(typeof(ItemView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemView>> UpdateItem(
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
                    actorAccessor.DomuActor.ActorId,
                    householdId,
                    spaceId,
                    itemId,
                    request.Name,
                    request.Category,
                    request.Barcode),
                cancellationToken);

            return Ok(item);
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
    [ProducesResponseType(typeof(ItemView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemView>> ReplaceItemEntries(
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
                    actorAccessor.DomuActor.ActorId,
                    householdId,
                    spaceId,
                    itemId,
                    request.Entries.Select(entry => entry.ToDraft()).ToArray()),
                cancellationToken);

            return Ok(item);
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
                new DeleteItemCommand(actorAccessor.DomuActor.ActorId, householdId, spaceId, itemId),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

}
