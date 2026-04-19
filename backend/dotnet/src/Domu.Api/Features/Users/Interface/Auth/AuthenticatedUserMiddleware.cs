using System.Security.Claims;
using Domu.Api.Features.Users.Application;
using Microsoft.Extensions.Options;

namespace Domu.Api.Features.Users.Interface.Auth;

public sealed class AuthenticatedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IEnsureUserUseCase ensureUserUseCase,
        IOptions<ExternalAuthenticationOptions> options)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            var externalIdentifier = context.User.FindFirstValue(options.Value.SubjectClaimType)
                                     ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(externalIdentifier))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var user = await ensureUserUseCase.ExecuteAsync(
                new UserAuthIdentity(externalIdentifier),
                context.RequestAborted);

            context.Items[UserAccessor.HttpContextItemKey] = user;
        }

        await next(context);
    }
}
