using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application;

public sealed class EnsureUserUseCase(IUserRepository userRepository) : IEnsureUserUseCase
{
    public async Task<User> ExecuteAsync(UserAuthIdentity authIdentity, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authIdentity.ExternalIdentifier);

        var existingUser = await userRepository.GetByAuthIdentityAsync(
            authIdentity.ExternalIdentifier,
            cancellationToken);
        if (existingUser is not null)
            return existingUser;

        var user = new User(Guid.CreateVersion7());
        await userRepository.AddAsync(user, authIdentity.ExternalIdentifier, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }
}
