using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Application;

public interface IEnsureUserUseCase
{
    Task<AuthenticatedUser> ExecuteAsync(UserAuthIdentity authIdentity, CancellationToken cancellationToken);
}
