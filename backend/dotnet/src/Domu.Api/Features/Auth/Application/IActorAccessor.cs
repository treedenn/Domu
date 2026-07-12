using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Auth.Application;

public interface IActorAccessor
{
    DomuActor DomuActor { get; }
}