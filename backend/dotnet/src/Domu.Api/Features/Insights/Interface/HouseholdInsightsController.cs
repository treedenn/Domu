using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Insights.Application;
using Domu.Api.Features.Insights.Application.Contracts;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Insights.Interface;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/insights")]
[Tags("Insights")]
public sealed class HouseholdInsightsController(
    IActorAccessor actorAccessor,
    IGetHouseholdInsightsUseCase getHouseholdInsightsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HouseholdInsightsView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HouseholdInsightsView>>> GetInsights(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var insights = await getHouseholdInsightsUseCase.ExecuteAsync(
                new GetHouseholdInsightsQuery(householdId, actorAccessor.DomuActor),
                cancellationToken);

            return Ok(new ApiResponse<HouseholdInsightsView>(insights));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
