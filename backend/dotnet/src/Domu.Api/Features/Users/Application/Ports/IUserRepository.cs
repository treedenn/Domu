using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application.Ports;

public interface IUserRepository
{
    Task<User?> GetByAuthIdentityAsync(string externalIdentifier, CancellationToken cancellationToken);
    Task AddAsync(User user, string externalIdentifier, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
