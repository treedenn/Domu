import 'package:flutter/material.dart';

import '../../app/theme/app_colors.dart';
import '../../features/items/domain/consumable_state.dart';

class StateChip extends StatelessWidget {
  const StateChip({
    required this.state,
    this.dense = false,
    super.key,
  });

  final ConsumableState state;
  final bool dense;

  @override
  Widget build(BuildContext context) {
    final AppSemanticColors colors =
        Theme.of(context).extension<AppSemanticColors>()!;
    final Color fill = switch (state) {
      ConsumableState.unopened => colors.stateUnopened,
      ConsumableState.opened => colors.stateOpened,
      ConsumableState.unknown => colors.stateUnknown,
    };
    final Color foreground = fill.computeLuminance() > 0.45
        ? Colors.black
        : Colors.white;
    final String label = switch (state) {
      ConsumableState.unopened => 'Unopened',
      ConsumableState.opened => 'Opened',
      ConsumableState.unknown => 'Unknown',
    };
    final IconData icon = switch (state) {
      ConsumableState.unopened => Icons.lock_outline,
      ConsumableState.opened => Icons.lock_open_outlined,
      ConsumableState.unknown => Icons.help_outline,
    };

    return Semantics(
      label: label,
      child: Chip(
        avatar: Icon(icon, size: dense ? 14 : 16, color: foreground),
        label: Text(label),
        labelStyle: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: foreground,
              fontWeight: FontWeight.w700,
            ),
        visualDensity: dense ? VisualDensity.compact : VisualDensity.standard,
        materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
        backgroundColor: fill,
        side: BorderSide.none,
      ),
    );
  }
}
