namespace Domu.Api.Features.Households.Domain.Households;

public sealed class Household
{
    public const int NameMaxLength = 100;

    private string _name = null!;

    public Household(Guid id, Guid ownerId, string name)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(id))
            : id;
        ChangeOwnership(ownerId);
        Rename(name);
    }

    public Household(
        Guid id,
        Guid ownerId,
        string name,
        HouseholdSubscriptionPlan subscriptionPlan,
        HouseholdSubscriptionStatus subscriptionStatus,
        DateTimeOffset? subscriptionCurrentPeriodEndsAt,
        DateTimeOffset? subscriptionCancelledAt)
        : this(id, ownerId, name)
    {
        RestoreSubscriptionState(
            subscriptionPlan,
            subscriptionStatus,
            subscriptionCurrentPeriodEndsAt,
            subscriptionCancelledAt);
    }

    public Guid Id { get; }
    public Guid OwnerId { get; private set; }
    public HouseholdSubscriptionPlan SubscriptionPlan { get; private set; } = HouseholdSubscriptionPlan.Free;
    public HouseholdSubscriptionStatus SubscriptionStatus { get; private set; } = HouseholdSubscriptionStatus.Active;
    public DateTimeOffset? SubscriptionCurrentPeriodEndsAt { get; private set; }
    public DateTimeOffset? SubscriptionCancelledAt { get; private set; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Household name cannot be null or whitespace.", nameof(name));
        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Household name cannot be longer than {NameMaxLength} characters.", nameof(name));

        Name = name;
    }
    
    public void ChangeOwnership(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId));
        OwnerId = ownerId;
    }

    public void ActivatePremiumSubscription(DateTimeOffset currentPeriodEndsAt, DateTimeOffset activatedAt)
    {
        if (currentPeriodEndsAt <= activatedAt)
            throw new ArgumentException("Subscription period end must be after activation time.", nameof(currentPeriodEndsAt));

        SubscriptionPlan = HouseholdSubscriptionPlan.Premium;
        SubscriptionStatus = HouseholdSubscriptionStatus.Active;
        SubscriptionCurrentPeriodEndsAt = currentPeriodEndsAt;
        SubscriptionCancelledAt = null;
    }

    public void ScheduleSubscriptionCancellation(DateTimeOffset cancelledAt)
    {
        if (SubscriptionPlan == HouseholdSubscriptionPlan.Free)
            throw new InvalidOperationException("Free households do not have a paid subscription to cancel.");
        if (SubscriptionCurrentPeriodEndsAt is null)
            throw new InvalidOperationException("Paid subscriptions must have a current period end.");
        if (cancelledAt >= SubscriptionCurrentPeriodEndsAt.Value)
            throw new ArgumentException("Cancellation time must be before the current period ends.", nameof(cancelledAt));

        SubscriptionStatus = HouseholdSubscriptionStatus.CancellationScheduled;
        SubscriptionCancelledAt = cancelledAt;
    }

    public void ExpireSubscription(DateTimeOffset expiredAt)
    {
        if (SubscriptionCurrentPeriodEndsAt is null)
            throw new InvalidOperationException("Subscription cannot expire without a current period end.");
        if (expiredAt < SubscriptionCurrentPeriodEndsAt.Value)
            throw new ArgumentException("Subscription cannot expire before the current period ends.", nameof(expiredAt));

        SubscriptionPlan = HouseholdSubscriptionPlan.Free;
        SubscriptionStatus = HouseholdSubscriptionStatus.Expired;
        SubscriptionCurrentPeriodEndsAt = null;
    }

    public bool HasPremiumAccessAt(DateTimeOffset timestamp)
    {
        return SubscriptionPlan == HouseholdSubscriptionPlan.Premium
               && SubscriptionCurrentPeriodEndsAt is not null
               && timestamp < SubscriptionCurrentPeriodEndsAt.Value;
    }

    private void RestoreSubscriptionState(
        HouseholdSubscriptionPlan subscriptionPlan,
        HouseholdSubscriptionStatus subscriptionStatus,
        DateTimeOffset? subscriptionCurrentPeriodEndsAt,
        DateTimeOffset? subscriptionCancelledAt)
    {
        if (subscriptionPlan == HouseholdSubscriptionPlan.Free && subscriptionCurrentPeriodEndsAt is not null)
            throw new ArgumentException("Free households cannot have a subscription period end.", nameof(subscriptionCurrentPeriodEndsAt));
        if (subscriptionPlan == HouseholdSubscriptionPlan.Premium && subscriptionCurrentPeriodEndsAt is null)
            throw new ArgumentException("Premium households must have a subscription period end.", nameof(subscriptionCurrentPeriodEndsAt));
        if (subscriptionStatus == HouseholdSubscriptionStatus.CancellationScheduled && subscriptionCancelledAt is null)
            throw new ArgumentException("Cancellation-scheduled households must have a cancellation time.", nameof(subscriptionCancelledAt));
        if (subscriptionStatus != HouseholdSubscriptionStatus.CancellationScheduled && subscriptionCancelledAt is not null)
            throw new ArgumentException("Only cancellation-scheduled households can have a cancellation time.", nameof(subscriptionCancelledAt));

        SubscriptionPlan = subscriptionPlan;
        SubscriptionStatus = subscriptionStatus;
        SubscriptionCurrentPeriodEndsAt = subscriptionCurrentPeriodEndsAt;
        SubscriptionCancelledAt = subscriptionCancelledAt;
    }
}
