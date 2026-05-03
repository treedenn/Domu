using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application.Ports;

public interface IUserRepository
{
    Task<AuthenticatedUser?> GetByAuthIdentityAsync(string externalIdentifier, CancellationToken cancellationToken);
    Task AddAsync(AuthenticatedUser authenticatedUser, string externalIdentifier, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
