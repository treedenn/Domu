class Household {
  const Household({
    required this.id,
    required this.name,
    required this.subscriptionPlan,
    required this.subscriptionStatus,
  });

  final String id;
  final String name;
  final String subscriptionPlan;
  final String subscriptionStatus;

  factory Household.fromJson(Map<String, Object?> json) {
    return Household(
      id: json['id'].toString(),
      name: json['name']?.toString() ?? 'Untitled household',
      subscriptionPlan: _subscriptionPlanLabel(json['subscriptionPlan']),
      subscriptionStatus: _subscriptionStatusLabel(json['subscriptionStatus']),
    );
  }

  static String _subscriptionPlanLabel(Object? value) {
    return switch (value) {
      String text when text.isNotEmpty => _humanize(text),
      0 => 'Free',
      1 => 'Premium',
      _ => 'Unknown',
    };
  }

  static String _subscriptionStatusLabel(Object? value) {
    return switch (value) {
      String text when text.isNotEmpty => _humanize(text),
      0 => 'Active',
      1 => 'Cancellation scheduled',
      2 => 'Expired',
      _ => 'Unknown',
    };
  }

  static String _humanize(String value) {
    final String withSpaces = value.replaceAllMapped(
      RegExp(r'(?<!^)([A-Z])'),
      (Match match) => ' ${match.group(1)}',
    );
    return withSpaces[0].toUpperCase() + withSpaces.substring(1);
  }
}
