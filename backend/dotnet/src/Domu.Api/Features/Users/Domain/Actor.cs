namespace Domu.Api.Features.Users.Domain;

public sealed class Actor
{
    private Actor()
    {
    }

    public Actor(Guid id, string externalIdentifier, SubscriptionTier subscriptionTier = SubscriptionTier.Default)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Actor id cannot be empty.", nameof(id))
            : id;
        ExternalIdentifier = string.IsNullOrWhiteSpace(externalIdentifier)
            ? throw new ArgumentException("External identifier cannot be empty.", nameof(externalIdentifier))
            : externalIdentifier;
        SubscriptionTier = subscriptionTier;
    }

    public Guid Id { get; private set; }

    public string ExternalIdentifier { get; private set; } = null!;

    public SubscriptionTier SubscriptionTier { get; private set; }

    public void ChangeSubscriptionTier(SubscriptionTier subscriptionTier)
    {
        SubscriptionTier = subscriptionTier;
    }
}
