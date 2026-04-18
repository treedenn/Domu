using Domu.Api.Features.Users.Domain;

namespace Domu.Api.Features.Locations.Domain.Locations;

public static class SubscriptionTierLimits
{
    public static int GetLocationLimit(SubscriptionTier tier)
    {
        return tier == SubscriptionTier.Premium ? 999 : 30;
    }

    public static int GetLocationMemberLimit(SubscriptionTier tier)
    {
        return tier == SubscriptionTier.Premium ? 8 : 2;
    }

    public static int GetLocationItemLimit(SubscriptionTier tier)
    {
        return tier == SubscriptionTier.Premium ? 999 : 30;
    }
}