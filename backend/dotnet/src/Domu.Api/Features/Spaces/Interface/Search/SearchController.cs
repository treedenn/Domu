using Domu.Api.Features.Spaces.Application.Search;
using Domu.Api.Features.Spaces.Application.Search.Contracts;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Search;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/search")]
[Tags("Search")]
public sealed class SearchController(
    IUserAccessor userAccessor,
    ISearchSpacesAndItemsUseCase searchSpacesAndItemsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultsView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SearchResultsView>> Search(
        Guid householdId,
        [FromQuery] string? text,
        [FromQuery] int? expiringWithinDays,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await searchSpacesAndItemsUseCase.ExecuteAsync(
                new SearchSpacesAndItemsQuery(userAccessor.User.Id, householdId, text, expiringWithinDays, limit),
                cancellationToken);

            return Ok(results);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid search request.", Detail = exception.Message });
        }
    }
}
