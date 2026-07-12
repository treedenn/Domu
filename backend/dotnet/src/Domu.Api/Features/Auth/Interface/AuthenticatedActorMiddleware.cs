using Domu.Api.Features.Auth.Application;

namespace Domu.Api.Features.Auth.Interface;

public sealed class AuthenticatedActorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IEnumerable<IActorResolver> actorResolvers)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            foreach (var resolver in actorResolvers)
            {
                if (!resolver.CanResolve(context.User))
                    continue;

                var actor = await resolver.ResolveAsync(context.User, context.RequestAborted);
                if (actor != null)
                {
                    context.Items[HttpContextActorAccessor.HttpContextItemKey] = actor;
                    break;
                }
            }

            if (!context.Items.ContainsKey(HttpContextActorAccessor.HttpContextItemKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}