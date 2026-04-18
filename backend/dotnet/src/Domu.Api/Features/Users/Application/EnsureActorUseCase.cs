using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application;

public sealed class EnsureActorUseCase(IActorRepository actorRepository) : IEnsureActorUseCase
{
    public async Task<Actor> ExecuteAsync(ExternalActorIdentity identity, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.ExternalIdentifier);

        var existingActor = await actorRepository.GetByExternalIdentifierAsync(
            identity.ExternalIdentifier,
            cancellationToken);
        if (existingActor is not null)
            return existingActor;

        var actor = new Actor(Guid.CreateVersion7(), identity.ExternalIdentifier);
        await actorRepository.AddAsync(actor, cancellationToken);
        await actorRepository.SaveChangesAsync(cancellationToken);

        return actor;
    }
}
