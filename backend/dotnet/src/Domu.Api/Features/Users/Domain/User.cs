namespace Domu.Api.Features.Users.Domain;

public sealed class User
{
    private User()
    {
    }

    public User(Guid id, SubscriptionTier subscriptionTier = SubscriptionTier.Default)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("User id cannot be empty.", nameof(id))
            : id;
        SubscriptionTier = subscriptionTier;
    }

    public Guid Id { get; private set; }

    public SubscriptionTier SubscriptionTier { get; private set; }

    public void ChangeSubscriptionTier(SubscriptionTier subscriptionTier)
    {
        SubscriptionTier = subscriptionTier;
    }
}
