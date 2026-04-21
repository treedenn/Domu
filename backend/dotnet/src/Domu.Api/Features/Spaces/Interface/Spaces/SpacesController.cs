using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Spaces;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/spaces")]
[Tags("Spaces")]
public sealed class SpacesController(
    IUserAccessor userAccessor,
    IHouseholdRepository householdRepository,
    ISpaceRepository spaceRepository,
    ICreateSpaceUseCase createSpaceUseCase,
    IGetSpaceUseCase getSpaceUseCase,
    IGetSpacesPageUseCase getSpacesPageUseCase,
    IUpdateSpaceUseCase updateSpaceUseCase,
    IMoveSpaceUseCase moveSpaceUseCase,
    IDeleteSpaceUseCase deleteSpaceUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SpacePage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpacePage>> GetSpaces(
        Guid householdId,
        [FromQuery] Guid? parentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeItems = false,
        [FromQuery] bool includeItemCount = false,
        [FromQuery] bool includeChildSpaces = false,
        [FromQuery] bool includeChildSpaceCount = false,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        if (parentId is not null && !await SpaceBelongsToHouseholdAsync(parentId.Value, householdId, cancellationToken))
            return NotFound();

        try
        {
            var page = await getSpacesPageUseCase.ExecuteAsync(
                new GetSpacesPageQuery(
                    householdId,
                    parentId,
                    pageNumber,
                    pageSize,
                    ResolveItemsProjection(includeItems, includeItemCount),
                    ResolveChildrenProjection(includeChildSpaces, includeChildSpaceCount)),
                cancellationToken);

            return Ok(page);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid page request.", Detail = exception.Message });
        }
    }

    [HttpGet("{spaceId:guid}")]
    [ProducesResponseType(typeof(SpaceView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceView>> GetSpace(
        Guid householdId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        try
        {
            var space = await getSpaceUseCase.ExecuteAsync(
                new GetSpaceQuery(spaceId, householdId),
                cancellationToken);

            return Ok(space);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpaceView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceView>> CreateSpace(
        Guid householdId,
        CreateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        if (request.ParentId is not null &&
            !await SpaceBelongsToHouseholdAsync(request.ParentId.Value, householdId, cancellationToken))
            return NotFound();

        try
        {
            var space = await createSpaceUseCase.ExecuteAsync(
                new CreateSpaceCommand(householdId, request.Name, request.Description, request.ParentId),
                cancellationToken);

            return CreatedAtAction(nameof(GetSpace), new { householdId, spaceId = space.Id }, space);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid space.", Detail = exception.Message });
        }
    }

    [HttpPut("{spaceId:guid}")]
    [ProducesResponseType(typeof(SpaceView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceView>> UpdateSpace(
        Guid householdId,
        Guid spaceId,
        UpdateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        if (!await SpaceBelongsToHouseholdAsync(spaceId, householdId, cancellationToken))
            return NotFound();

        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        try
        {
            var space = await updateSpaceUseCase.ExecuteAsync(
                new UpdateSpaceCommand(spaceId, request.Name, request.Description),
                cancellationToken);

            return Ok(space);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid space.", Detail = exception.Message });
        }
    }

    [HttpPut("{spaceId:guid}/parent")]
    [ProducesResponseType(typeof(SpaceView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceView>> MoveSpace(
        Guid householdId,
        Guid spaceId,
        MoveSpaceRequest request,
        CancellationToken cancellationToken)
    {
        if (!await SpaceBelongsToHouseholdAsync(spaceId, householdId, cancellationToken))
            return NotFound();

        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        if (request.ParentId is not null &&
            !await SpaceBelongsToHouseholdAsync(request.ParentId.Value, householdId, cancellationToken))
            return NotFound();

        try
        {
            var space = await moveSpaceUseCase.ExecuteAsync(
                new MoveSpaceCommand(spaceId, request.ParentId),
                cancellationToken);

            return Ok(space);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid space move.", Detail = exception.Message });
        }
    }

    [HttpDelete("{spaceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSpace(Guid householdId, Guid spaceId, CancellationToken cancellationToken)
    {
        if (!await SpaceBelongsToHouseholdAsync(spaceId, householdId, cancellationToken))
            return NotFound();

        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        try
        {
            await deleteSpaceUseCase.ExecuteAsync(new DeleteSpaceCommand(spaceId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<bool> CanAccessHouseholdAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken);
        return household?.OwnerId == userAccessor.User.Id;
    }

    private async Task<bool> SpaceBelongsToHouseholdAsync(
        Guid spaceId,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        var space = await spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        return space?.HouseholdId == householdId;
    }

    private static SpaceItemsProjection ResolveItemsProjection(bool includeItems, bool includeItemCount)
    {
        if (includeItems)
            return SpaceItemsProjection.Data;

        return includeItemCount ? SpaceItemsProjection.Count : SpaceItemsProjection.None;
    }

    private static SpaceChildrenProjection ResolveChildrenProjection(
        bool includeChildSpaces,
        bool includeChildSpaceCount)
    {
        if (includeChildSpaces)
            return SpaceChildrenProjection.Data;

        return includeChildSpaceCount ? SpaceChildrenProjection.Count : SpaceChildrenProjection.None;
    }
}
