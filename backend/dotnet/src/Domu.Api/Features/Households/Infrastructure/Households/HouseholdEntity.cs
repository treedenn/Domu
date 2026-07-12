using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Infrastructure.Households;

public sealed class HouseholdEntity
{
    private HouseholdEntity()
    {
    }

    public HouseholdEntity(
        Guid id,
        string name,
        HouseholdSubscriptionPlan subscriptionPlan,
        HouseholdSubscriptionStatus subscriptionStatus,
        DateTimeOffset? subscriptionCurrentPeriodEndsAt,
        DateTimeOffset? subscriptionCancelledAt)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(id))
            : id;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Household name cannot be empty.", nameof(name))
            : name;
        SubscriptionPlan = subscriptionPlan;
        SubscriptionStatus = subscriptionStatus;
        SubscriptionCurrentPeriodEndsAt = subscriptionCurrentPeriodEndsAt;
        SubscriptionCancelledAt = subscriptionCancelledAt;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public HouseholdSubscriptionPlan SubscriptionPlan { get; private set; }
    public HouseholdSubscriptionStatus SubscriptionStatus { get; private set; }
    public DateTimeOffset? SubscriptionCurrentPeriodEndsAt { get; private set; }
    public DateTimeOffset? SubscriptionCancelledAt { get; private set; }

    public Household ToDomain()
    {
        return new Household(
            Id,
            Name,
            SubscriptionPlan,
            SubscriptionStatus,
            SubscriptionCurrentPeriodEndsAt,
            SubscriptionCancelledAt);
    }

    public static HouseholdEntity FromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);

        return new HouseholdEntity(
            household.Id,
            household.Name,
            household.SubscriptionPlan,
            household.SubscriptionStatus,
            household.SubscriptionCurrentPeriodEndsAt,
            household.SubscriptionCancelledAt);
    }

    public void UpdateFromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);
        if (household.Id != Id)
            throw new ArgumentException("Cannot update household entity from a different household.",
                nameof(household));

        Name = household.Name;
        SubscriptionPlan = household.SubscriptionPlan;
        SubscriptionStatus = household.SubscriptionStatus;
        SubscriptionCurrentPeriodEndsAt = household.SubscriptionCurrentPeriodEndsAt;
        SubscriptionCancelledAt = household.SubscriptionCancelledAt;
    }
}
