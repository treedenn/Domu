using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Users.Infrastructure;

public sealed class UserEntity
{
    private UserEntity()
    {
    }

    public UserEntity(Guid id, string externalIdentifier)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(id))
            : id;
        ExternalIdentifier = string.IsNullOrWhiteSpace(externalIdentifier)
            ? throw new ArgumentException("External identifier cannot be empty.", nameof(externalIdentifier))
            : externalIdentifier;
    }

    public Guid Id { get; private set; }
    public string ExternalIdentifier { get; private set; } = null!;

    public AuthenticatedUser ToDomain()
    {
        return new AuthenticatedUser(Id);
    }

    public static UserEntity FromDomain(AuthenticatedUser authenticatedUser, string externalIdentifier)
    {
        ArgumentNullException.ThrowIfNull(authenticatedUser);

        return new UserEntity(authenticatedUser.Id, externalIdentifier);
    }
}
