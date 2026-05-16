using Domu.Api.Features.Insights.Application;
using Domu.Api.Features.Insights.Application.Contracts;
using Domu.Api.Features.Users.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Insights.Interface;

[ApiController]
[Authorize]
[Route("households/{householdId:guid}/insights")]
[Tags("Insights")]
public sealed class HouseholdInsightsController(
    IUserAccessor userAccessor,
    IGetHouseholdInsightsUseCase getHouseholdInsightsUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HouseholdInsightsView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdInsightsView>> GetInsights(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var insights = await getHouseholdInsightsUseCase.ExecuteAsync(
                new GetHouseholdInsightsQuery(householdId, userAccessor.User.Id),
                cancellationToken);

            return Ok(insights);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
