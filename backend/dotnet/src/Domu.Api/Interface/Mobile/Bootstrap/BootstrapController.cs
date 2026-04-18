using Domu.Api.Interface.Mobile.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Interface.Mobile.Bootstrap;

[ApiController]
[Route("app")]
[AllowAnonymous]
[Tags("App")]
public sealed class BootstrapController(IAppBootstrapService appBootstrapService) : ControllerBase
{
    // [HttpGet("bootstrap")]
    // [ProducesResponseType(typeof(HttpResponse<AppBootstrapResponse>), StatusCodes.Status200OK)]
    // public IActionResult Bootstrap()
    // {
    //     var response = appBootstrapService.BuildResponse();
    //     return Ok(new HttpResponse<AppBootstrapResponse>(response));
    // }
}
