using System.Security.Claims;
using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Users.Application;
using Microsoft.Extensions.Options;

namespace Domu.Api.Features.Users.Interface.Auth;

public sealed class ZitadelActorResolver(
    IEnsureUserUseCase ensureUserUseCase,
    IOptions<JwtAuthenticationOptions> options) : IActorResolver
{
    public string AuthenticationSchema => "Zitadel";

    public bool CanResolve(ClaimsPrincipal principal)
    {
        return true;
    }

    public async Task<DomuActor?> ResolveAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var externalIdentifier = principal.FindFirstValue(options.Value.SubjectClaimType)
                                 ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(externalIdentifier))
            return null;

        var user = await ensureUserUseCase.ExecuteAsync(new UserAuthIdentity(externalIdentifier), cancellationToken);

        return new DomuActor(user.Id, DomuActorType.Zitadel);
    }
}