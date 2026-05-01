import 'package:flutter/material.dart';

import '../../app/theme/tokens.dart';

class AppCard extends StatelessWidget {
  const AppCard({
    required this.child,
    this.onTap,
    this.padding = const EdgeInsets.all(AppSpacing.lg),
    this.tonal = false,
    super.key,
  });

  final Widget child;
  final VoidCallback? onTap;
  final EdgeInsetsGeometry padding;
  final bool tonal;

  @override
  Widget build(BuildContext context) {
    final ColorScheme colorScheme = Theme.of(context).colorScheme;
    final Widget content = Padding(padding: padding, child: child);

    return Card(
      color: tonal
          ? colorScheme.surfaceContainerHighest
          : colorScheme.surfaceContainerLow,
      child: onTap == null
          ? content
          : InkWell(
              onTap: onTap,
              borderRadius: const BorderRadius.all(AppRadii.lg),
              child: content,
            ),
    );
  }
}
