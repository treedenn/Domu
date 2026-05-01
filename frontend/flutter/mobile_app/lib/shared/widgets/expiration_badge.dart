import 'package:flutter/material.dart';

import '../../app/theme/app_colors.dart';

class ExpirationBadge extends StatelessWidget {
  const ExpirationBadge({
    required this.expiresAt,
    this.verbose = false,
    super.key,
  });

  final DateTime? expiresAt;
  final bool verbose;

  @override
  Widget build(BuildContext context) {
    final DateTime? date = expiresAt;
    if (date == null && !verbose) {
      return const SizedBox.shrink();
    }

    final AppSemanticColors semanticColors =
        Theme.of(context).extension<AppSemanticColors>()!;
    final DateTime today = DateTime.now();
    final DateTime normalizedToday = DateTime(today.year, today.month, today.day);
    final DateTime normalizedDate = date == null
        ? normalizedToday
        : DateTime(date.year, date.month, date.day);
    final int days = normalizedDate.difference(normalizedToday).inDays;
    final String label;
    final Color color;

    if (date == null) {
      label = 'No expiry';
      color = Theme.of(context).colorScheme.outline;
    } else if (days < 0) {
      label = 'Expired ${days.abs()}d ago';
      color = semanticColors.expired;
    } else if (days <= 7) {
      label = days == 0 ? 'Expires today' : 'Expires in ${days}d';
      color = semanticColors.expiringSoon;
    } else {
      label = 'Expires ${date.month}/${date.day}/${date.year}';
      color = Theme.of(context).colorScheme.outline;
    }

    final Color foreground = color.computeLuminance() > 0.45
        ? Colors.black
        : Colors.white;

    return Semantics(
      label: label,
      child: Chip(
        label: Text(label),
        labelStyle: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: foreground,
              fontWeight: FontWeight.w700,
            ),
        visualDensity: VisualDensity.compact,
        materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
        backgroundColor: color,
        side: BorderSide.none,
      ),
    );
  }
}
