using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Spaces.Application.Expirations;
using Domu.Api.Features.Spaces.Application.Expirations.Contracts;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Spaces.Interface.Expirations;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/expirations")]
[Tags("Expirations")]
public sealed class HouseholdExpirationsController(
    IActorAccessor actorAccessor,
    GetHouseholdExpirationsUseCase getHouseholdExpirationsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HouseholdExpirationsView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HouseholdExpirationsView>>> GetExpirations(
        Guid householdId,
        [FromQuery] DateTimeOffset? upcomingUntilUtc,
        CancellationToken cancellationToken)
    {
        if (upcomingUntilUtc is null)
            return BadRequest(new ProblemDetails { Title = "Invalid expiration request.", Detail = "upcomingUntilUtc is required." });

        try
        {
            var expirations = await getHouseholdExpirationsUseCase.ExecuteAsync(
                new GetHouseholdExpirationsQuery(actorAccessor.DomuActor, householdId, upcomingUntilUtc.Value),
                cancellationToken);
            return Ok(new ApiResponse<HouseholdExpirationsView>(expirations));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid expiration request.", Detail = exception.Message });
        }
    }
}
