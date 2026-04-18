using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Interface.Auth;

public interface IActorAccessor
{
    Actor Actor { get; }
}

public sealed class ActorAccessor(IHttpContextAccessor httpContextAccessor) : IActorAccessor
{
    public const string HttpContextItemKey = "AppUser";

    public Actor Actor => (httpContextAccessor.HttpContext!.Items[HttpContextItemKey] as Actor)!;
}
