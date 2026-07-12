using System.Security.Claims;
using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Auth.Application;

public interface IActorResolver
{
    string AuthenticationSchema { get; }

    bool CanResolve(ClaimsPrincipal principal);
    Task<DomuActor?> ResolveAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}