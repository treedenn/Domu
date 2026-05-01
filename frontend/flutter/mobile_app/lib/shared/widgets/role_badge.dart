import 'package:flutter/material.dart';

import '../../features/households/domain/member.dart';

class RoleBadge extends StatelessWidget {
  const RoleBadge({required this.role, super.key});

  final MemberRole role;

  @override
  Widget build(BuildContext context) {
    final ColorScheme scheme = Theme.of(context).colorScheme;
    final Color color = switch (role) {
      MemberRole.owner => scheme.primaryContainer,
      MemberRole.admin => scheme.secondaryContainer,
      MemberRole.member => scheme.surfaceContainerHighest,
    };
    final String label = switch (role) {
      MemberRole.owner => 'Owner',
      MemberRole.admin => 'Admin',
      MemberRole.member => 'Member',
    };

    return Chip(
      label: Text(label),
      visualDensity: VisualDensity.compact,
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
      backgroundColor: color,
    );
  }
}
