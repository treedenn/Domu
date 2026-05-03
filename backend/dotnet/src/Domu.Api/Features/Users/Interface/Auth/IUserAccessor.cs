using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Interface.Auth;

public interface IUserAccessor
{
    AuthenticatedUser User { get; }
}

public sealed class UserAccessor(IHttpContextAccessor httpContextAccessor) : IUserAccessor
{
    public const string HttpContextItemKey = "CurrentUser";

    public AuthenticatedUser User => (httpContextAccessor.HttpContext!.Items[HttpContextItemKey] as AuthenticatedUser)!;
}
