namespace Domu.Api.Features.Auth.Domain;

public sealed class DomuActor(Guid actorId, DomuActorType actorType)
{
    public Guid ActorId { get; } = actorId;
    public DomuActorType ActorType { get; } = actorType;
}