using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Users.Infrastructure;

public sealed class ActorRepository(AppDbContext dbContext) : IActorRepository
{
    public Task<Actor?> GetByExternalIdentifierAsync(string externalIdentifier, CancellationToken cancellationToken)
    {
        return dbContext.Actors
            .SingleOrDefaultAsync(actor => actor.ExternalIdentifier == externalIdentifier, cancellationToken);
    }

    public async Task AddAsync(Actor actor, CancellationToken cancellationToken)
    {
        await dbContext.Actors.AddAsync(actor, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
