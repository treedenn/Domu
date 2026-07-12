using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Users.Infrastructure;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<AuthenticatedUser?> GetByAuthIdentityAsync(string externalIdentifier,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(user => user.ExternalIdentifier == externalIdentifier, cancellationToken);

        return user?.ToDomain();
    }

    public async Task AddAsync(AuthenticatedUser authenticatedUser, string externalIdentifier,
        CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(UserEntity.FromDomain(authenticatedUser, externalIdentifier), cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}