using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Application.Households.Contracts;

public sealed record HouseholdView(
    Guid Id,
    Guid OwnerMemberId,
    string Name,
    HouseholdSubscriptionPlan SubscriptionPlan,
    HouseholdSubscriptionStatus SubscriptionStatus,
    DateTimeOffset? SubscriptionCurrentPeriodEndsAt,
    DateTimeOffset? SubscriptionCancelledAt)
{
    public static HouseholdView FromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);

        return new HouseholdView(
            household.Id,
            household.OwnerMemberId ?? throw new InvalidOperationException("Household owner is not assigned."),
            household.Name,
            household.SubscriptionPlan,
            household.SubscriptionStatus,
            household.SubscriptionCurrentPeriodEndsAt,
            household.SubscriptionCancelledAt);
    }
}
