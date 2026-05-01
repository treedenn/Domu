using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;
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
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
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
        if (!await CanAccessHouseholdAsync(householdId, cancellationToken))
            return NotFound();

        try
        {
            var results = await searchSpacesAndItemsUseCase.ExecuteAsync(
                new SearchSpacesAndItemsQuery(householdId, text, expiringWithinDays, limit),
                cancellationToken);

            return Ok(results);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid search request.", Detail = exception.Message });
        }
    }

    private async Task<bool> CanAccessHouseholdAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var household = await householdRepository.GetByIdAsync(householdId, cancellationToken);
        return household?.OwnerId == userAccessor.User.Id
               || await membershipRepository.IsMemberAsync(householdId, userAccessor.User.Id, cancellationToken);
    }
}
