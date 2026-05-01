using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Items;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/spaces/{spaceId:guid}/items")]
[Tags("Items")]
public sealed class ItemsController(
    IUserAccessor userAccessor,
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    ISpaceRepository spaceRepository,
    IItemRepository itemRepository,
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
        if (!await CanAccessSpaceAsync(householdId, spaceId, cancellationToken))
            return NotFound();

        var items = await getSpaceItemsUseCase.ExecuteAsync(spaceId, cancellationToken);
        return Ok(items);
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
        if (!await CanAccessSpaceAsync(householdId, spaceId, cancellationToken))
            return NotFound();

        try
        {
            var item = await createItemUseCase.ExecuteAsync(
                new CreateItemCommand(
                    spaceId,
                    request.Name,
                    request.Category,
                    request.Barcode,
                    request.Entries?.Select(entry => entry.ToDraft()).ToArray()),
                cancellationToken);

            return CreatedAtAction(nameof(GetItems), new { householdId, spaceId }, item);
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
        if (!await CanAccessItemAsync(householdId, spaceId, itemId, cancellationToken))
            return NotFound();

        try
        {
            var item = await updateItemUseCase.ExecuteAsync(
                new UpdateItemCommand(
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
        if (!await CanAccessItemAsync(householdId, spaceId, itemId, cancellationToken))
            return NotFound();

        try
        {
            var item = await replaceItemEntriesUseCase.ExecuteAsync(
                new ReplaceItemEntriesCommand(
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
        if (!await CanAccessItemAsync(householdId, spaceId, itemId, cancellationToken))
            return NotFound();

        try
        {
            await deleteItemUseCase.ExecuteAsync(new DeleteItemCommand(itemId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<bool> CanAccessItemAsync(
        Guid householdId,
        Guid spaceId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessSpaceAsync(householdId, spaceId, cancellationToken))
            return false;

        var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
        return item?.SpaceId == spaceId;
    }

    private async Task<bool> CanAccessSpaceAsync(Guid householdId, Guid spaceId, CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken);
        if (household is null)
            return false;

        if (household.OwnerId != userAccessor.User.Id
            && !await membershipRepository.IsMemberAsync(householdId, userAccessor.User.Id, cancellationToken))
            return false;

        var space = await spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        return space?.HouseholdId == householdId;
    }
}
