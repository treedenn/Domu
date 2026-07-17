using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Spaces;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/spaces")]
[Tags("Spaces")]
public sealed class SpacesController(
    IActorAccessor actorAccessor,
    ICreateSpaceUseCase createSpaceUseCase,
    IGetSpaceUseCase getSpaceUseCase,
    IGetSpacesPageUseCase getSpacesPageUseCase,
    IUpdateSpaceUseCase updateSpaceUseCase,
    IMoveSpaceUseCase moveSpaceUseCase,
    IDeleteSpaceUseCase deleteSpaceUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SpacePage>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpacePage>>> GetSpaces(
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
        try
        {
            var page = await getSpacesPageUseCase.ExecuteAsync(
                new GetSpacesPageQuery(
                    actorAccessor.DomuActor,
                    householdId,
                    parentId,
                    pageNumber,
                    pageSize,
                    ResolveItemsProjection(includeItems, includeItemCount),
                    ResolveChildrenProjection(includeChildSpaces, includeChildSpaceCount)),
                cancellationToken);

            return Ok(new ApiResponse<SpacePage>(page));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid page request.", Detail = exception.Message });
        }
    }

    [HttpGet("{spaceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SpaceView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpaceView>>> GetSpace(
        Guid householdId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await getSpaceUseCase.ExecuteAsync(
                new GetSpaceQuery(actorAccessor.DomuActor, householdId, spaceId),
                cancellationToken);

            return Ok(new ApiResponse<SpaceView>(space));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SpaceView>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpaceView>>> CreateSpace(
        Guid householdId,
        CreateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await createSpaceUseCase.ExecuteAsync(
                new CreateSpaceCommand(actorAccessor.DomuActor, householdId, request.Name, request.Description,
                    request.ParentId),
                cancellationToken);

            return CreatedAtAction(nameof(GetSpace), new { householdId, spaceId = space.Id }, new ApiResponse<SpaceView>(space));
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

    [HttpPut("{spaceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SpaceView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpaceView>>> UpdateSpace(
        Guid householdId,
        Guid spaceId,
        UpdateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await updateSpaceUseCase.ExecuteAsync(
                new UpdateSpaceCommand(actorAccessor.DomuActor, householdId, spaceId, request.Name,
                    request.Description),
                cancellationToken);

            return Ok(new ApiResponse<SpaceView>(space));
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
    [ProducesResponseType(typeof(ApiResponse<SpaceView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpaceView>>> MoveSpace(
        Guid householdId,
        Guid spaceId,
        MoveSpaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await moveSpaceUseCase.ExecuteAsync(
                new MoveSpaceCommand(actorAccessor.DomuActor, householdId, spaceId, request.ParentId),
                cancellationToken);

            return Ok(new ApiResponse<SpaceView>(space));
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSpace(Guid householdId, Guid spaceId, CancellationToken cancellationToken)
    {
        try
        {
            await deleteSpaceUseCase.ExecuteAsync(
                new DeleteSpaceCommand(actorAccessor.DomuActor, householdId, spaceId),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (SpaceNotEmptyException exception)
        {
            return Conflict(new ProblemDetails { Title = "Space is not empty.", Detail = exception.Message });
        }
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
