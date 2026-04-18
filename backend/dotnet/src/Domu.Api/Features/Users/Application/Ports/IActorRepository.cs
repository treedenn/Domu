using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application.Ports;

public interface IActorRepository
{
    Task<Actor?> GetByExternalIdentifierAsync(string externalIdentifier, CancellationToken cancellationToken);
    Task AddAsync(Actor actor, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
