import '../domain/household.dart';

class HouseholdDto {
  const HouseholdDto({
    required this.id,
    required this.name,
    required this.subscriptionPlan,
    required this.subscriptionStatus,
    this.subscriptionCurrentPeriodEndsAt,
    this.subscriptionCancelledAt,
  });

  factory HouseholdDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final name = json['name'];
    final plan = json['subscriptionPlan'];
    final status = json['subscriptionStatus'];
    if (id is! String ||
        id.isEmpty ||
        name is! String ||
        plan is! String ||
        status is! String) {
      throw const FormatException('Invalid household response.');
    }
    return HouseholdDto(
      id: id,
      name: name,
      subscriptionPlan: _planFromJson(plan),
      subscriptionStatus: _statusFromJson(status),
      subscriptionCurrentPeriodEndsAt: _dateFromJson(
        json['subscriptionCurrentPeriodEndsAt'],
      ),
      subscriptionCancelledAt: _dateFromJson(json['subscriptionCancelledAt']),
    );
  }

  final String id;
  final String name;
  final HouseholdSubscriptionPlan subscriptionPlan;
  final HouseholdSubscriptionStatus subscriptionStatus;
  final DateTime? subscriptionCurrentPeriodEndsAt;
  final DateTime? subscriptionCancelledAt;

  Household toDomain() => Household(
    id: id,
    name: name,
    subscriptionPlan: subscriptionPlan,
    subscriptionStatus: subscriptionStatus,
    subscriptionCurrentPeriodEndsAt: subscriptionCurrentPeriodEndsAt,
    subscriptionCancelledAt: subscriptionCancelledAt,
  );

  static HouseholdSubscriptionPlan _planFromJson(String value) =>
      switch (value) {
        'free' => HouseholdSubscriptionPlan.free,
        'premium' => HouseholdSubscriptionPlan.premium,
        'unknown' => HouseholdSubscriptionPlan.unknown,
        _ => throw const FormatException('Invalid household response.'),
      };

  static HouseholdSubscriptionStatus _statusFromJson(String value) =>
      switch (value) {
        'active' => HouseholdSubscriptionStatus.active,
        'cancellationScheduled' =>
          HouseholdSubscriptionStatus.cancellationScheduled,
        'expired' => HouseholdSubscriptionStatus.expired,
        'unknown' => HouseholdSubscriptionStatus.unknown,
        _ => throw const FormatException('Invalid household response.'),
      };

  static DateTime? _dateFromJson(Object? value) {
    if (value == null) {
      return null;
    }
    if (value is! String) {
      throw const FormatException('Invalid household response.');
    }
    final parsed = DateTime.tryParse(value);
    if (parsed == null) {
      throw const FormatException('Invalid household response.');
    }
    return parsed;
  }
}
