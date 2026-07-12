using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Domain;

public sealed class HouseholdUnitTest
{
    [Fact]
    public void Constructor_WithEmptyId_Throws()
    {
        var action = () => new Household(Guid.Empty, "Home");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Household id cannot be empty.", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_Throws()
    {
        var action = () => new Household(Guid.NewGuid(), " ");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Household name cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Rename_WithTooLongName_Throws()
    {
        var household = new Household(Guid.NewGuid(), "Home");
        var tooLongName = new string('A', Household.NameMaxLength + 1);

        var action = () => household.Rename(tooLongName);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains($"Household name cannot be longer than {Household.NameMaxLength} characters.",
            exception.Message);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var household = new Household(Guid.NewGuid(), "Home");

        household.Rename("Apartment");

        Assert.Equal("Apartment", household.Name);
    }

    [Fact]
    public void Constructor_DefaultsToFreeActiveSubscription()
    {
        var household = new Household(Guid.NewGuid(), "Home");

        Assert.Equal(HouseholdSubscriptionPlan.Free, household.SubscriptionPlan);
        Assert.Equal(HouseholdSubscriptionStatus.Active, household.SubscriptionStatus);
        Assert.Null(household.SubscriptionCurrentPeriodEndsAt);
        Assert.Null(household.SubscriptionCancelledAt);
    }

    [Fact]
    public void ActivatePremiumSubscription_WithFuturePeriodEnd_EnablesPremiumAccess()
    {
        var household = new Household(Guid.NewGuid(), "Home");
        var activatedAt = DateTimeOffset.UtcNow;
        var periodEnd = activatedAt.AddMonths(1);

        household.ActivatePremiumSubscription(periodEnd, activatedAt);

        Assert.Equal(HouseholdSubscriptionPlan.Premium, household.SubscriptionPlan);
        Assert.Equal(HouseholdSubscriptionStatus.Active, household.SubscriptionStatus);
        Assert.Equal(periodEnd, household.SubscriptionCurrentPeriodEndsAt);
        Assert.True(household.HasPremiumAccessAt(activatedAt.AddDays(1)));
    }

    [Fact]
    public void ScheduleSubscriptionCancellation_KeepsPremiumAccessUntilPeriodEnd()
    {
        var household = new Household(Guid.NewGuid(), "Home");
        var activatedAt = DateTimeOffset.UtcNow;
        var periodEnd = activatedAt.AddMonths(1);
        var cancelledAt = activatedAt.AddDays(10);

        household.ActivatePremiumSubscription(periodEnd, activatedAt);
        household.ScheduleSubscriptionCancellation(cancelledAt);

        Assert.Equal(HouseholdSubscriptionStatus.CancellationScheduled, household.SubscriptionStatus);
        Assert.Equal(cancelledAt, household.SubscriptionCancelledAt);
        Assert.True(household.HasPremiumAccessAt(periodEnd.AddTicks(-1)));
        Assert.False(household.HasPremiumAccessAt(periodEnd));
    }

    [Fact]
    public void ExpireSubscription_AfterPeriodEnd_DowngradesToFree()
    {
        var household = new Household(Guid.NewGuid(), "Home");
        var activatedAt = DateTimeOffset.UtcNow;
        var periodEnd = activatedAt.AddMonths(1);

        household.ActivatePremiumSubscription(periodEnd, activatedAt);
        household.ExpireSubscription(periodEnd);

        Assert.Equal(HouseholdSubscriptionPlan.Free, household.SubscriptionPlan);
        Assert.Equal(HouseholdSubscriptionStatus.Expired, household.SubscriptionStatus);
        Assert.Null(household.SubscriptionCurrentPeriodEndsAt);
        Assert.False(household.HasPremiumAccessAt(periodEnd));
    }
}
