enum HouseholdSubscriptionPlan { unknown, free, premium }

enum HouseholdSubscriptionStatus {
  unknown,
  active,
  cancellationScheduled,
  expired,
}

class Household {
  const Household({
    required this.id,
    required this.name,
    required this.subscriptionPlan,
    required this.subscriptionStatus,
    this.subscriptionCurrentPeriodEndsAt,
    this.subscriptionCancelledAt,
  });

  final String id;
  final String name;
  final HouseholdSubscriptionPlan subscriptionPlan;
  final HouseholdSubscriptionStatus subscriptionStatus;
  final DateTime? subscriptionCurrentPeriodEndsAt;
  final DateTime? subscriptionCancelledAt;
}
