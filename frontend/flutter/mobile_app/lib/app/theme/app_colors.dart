import 'package:flutter/material.dart';

@immutable
class AppSemanticColors extends ThemeExtension<AppSemanticColors> {
  const AppSemanticColors({
    required this.stateUnopened,
    required this.stateOpened,
    required this.stateUnknown,
    required this.expired,
    required this.expiringSoon,
  });

  final Color stateUnopened;
  final Color stateOpened;
  final Color stateUnknown;
  final Color expired;
  final Color expiringSoon;

  factory AppSemanticColors.fromScheme(
    ColorScheme colorScheme,
    Brightness brightness,
  ) {
    return AppSemanticColors(
      stateUnopened: colorScheme.primary,
      stateOpened: colorScheme.tertiary,
      stateUnknown: colorScheme.outline,
      expired: colorScheme.error,
      expiringSoon: brightness == Brightness.light
          ? const Color(0xFFB8860B)
          : const Color(0xFFE0A82E),
    );
  }

  @override
  AppSemanticColors copyWith({
    Color? stateUnopened,
    Color? stateOpened,
    Color? stateUnknown,
    Color? expired,
    Color? expiringSoon,
  }) {
    return AppSemanticColors(
      stateUnopened: stateUnopened ?? this.stateUnopened,
      stateOpened: stateOpened ?? this.stateOpened,
      stateUnknown: stateUnknown ?? this.stateUnknown,
      expired: expired ?? this.expired,
      expiringSoon: expiringSoon ?? this.expiringSoon,
    );
  }

  @override
  AppSemanticColors lerp(ThemeExtension<AppSemanticColors>? other, double t) {
    if (other is! AppSemanticColors) {
      return this;
    }

    return AppSemanticColors(
      stateUnopened: Color.lerp(stateUnopened, other.stateUnopened, t)!,
      stateOpened: Color.lerp(stateOpened, other.stateOpened, t)!,
      stateUnknown: Color.lerp(stateUnknown, other.stateUnknown, t)!,
      expired: Color.lerp(expired, other.expired, t)!,
      expiringSoon: Color.lerp(expiringSoon, other.expiringSoon, t)!,
    );
  }
}
