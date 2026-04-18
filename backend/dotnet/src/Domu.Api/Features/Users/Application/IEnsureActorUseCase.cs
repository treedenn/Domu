using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application;

public interface IEnsureActorUseCase
{
    Task<Actor> ExecuteAsync(ExternalActorIdentity identity, CancellationToken cancellationToken);
}
