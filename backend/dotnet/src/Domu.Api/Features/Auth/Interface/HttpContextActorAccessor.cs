using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Auth.Interface;

public sealed class HttpContextActorAccessor(IHttpContextAccessor httpContextAccessor) : IActorAccessor
{
    internal const string HttpContextItemKey = "Actor";

    public DomuActor DomuActor =>
        httpContextAccessor.HttpContext?.Items[HttpContextItemKey] as DomuActor
        ?? throw new InvalidOperationException("No authenticated actor is available.");
}
