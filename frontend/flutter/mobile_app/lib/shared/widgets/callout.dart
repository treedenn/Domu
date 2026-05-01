import 'package:flutter/material.dart';

import '../../app/theme/tokens.dart';
import 'app_card.dart';

enum CalloutSeverity { info, warning }

class Callout extends StatelessWidget {
  const Callout({
    required this.message,
    this.actionLabel,
    this.onAction,
    this.severity = CalloutSeverity.info,
    super.key,
  });

  final String message;
  final String? actionLabel;
  final VoidCallback? onAction;
  final CalloutSeverity severity;

  @override
  Widget build(BuildContext context) {
    final ColorScheme scheme = Theme.of(context).colorScheme;
    return AppCard(
      tonal: true,
      child: Row(
        children: <Widget>[
          Icon(
            severity == CalloutSeverity.warning
                ? Icons.warning_amber_outlined
                : Icons.info_outline,
            color: severity == CalloutSeverity.warning
                ? scheme.tertiary
                : scheme.primary,
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(child: Text(message)),
          if (actionLabel != null)
            TextButton(
              onPressed: onAction,
              child: Text(actionLabel!),
            ),
        ],
      ),
    );
  }
}
