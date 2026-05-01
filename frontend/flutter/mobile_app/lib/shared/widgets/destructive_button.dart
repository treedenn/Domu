import 'package:flutter/material.dart';

class DestructiveButton extends StatelessWidget {
  const DestructiveButton({
    required this.onPressed,
    required this.child,
    super.key,
  });

  final VoidCallback? onPressed;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final ColorScheme scheme = Theme.of(context).colorScheme;
    return FilledButton.tonal(
      onPressed: onPressed,
      style: FilledButton.styleFrom(
        backgroundColor: scheme.errorContainer,
        foregroundColor: scheme.onErrorContainer,
      ),
      child: child,
    );
  }
}
