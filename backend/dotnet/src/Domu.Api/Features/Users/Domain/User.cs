namespace Domu.Api.Features.Users.Domain;

public sealed class User
{
    public User(Guid id)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(id))
            : id;
    }

    public Guid Id { get; private set; }
}
