import 'package:flutter/material.dart';

import '../../domain/household_member.dart';

class HouseholdMemberTile extends StatelessWidget {
  const HouseholdMemberTile({
    super.key,
    required this.member,
    required this.canManageMembers,
    required this.onRemove,
  });

  final HouseholdMember member;
  final bool canManageMembers;
  final ValueChanged<HouseholdMember> onRemove;

  @override
  Widget build(BuildContext context) => ListTile(
    key: ValueKey('member-${member.id}'),
    leading: Icon(
      member.role == HouseholdMemberRole.owner
          ? Icons.workspace_premium_outlined
          : Icons.person_outline,
    ),
    title: Text(member.displayName),
    subtitle: Text(member.role.label),
    trailing: canManageMembers && member.role != HouseholdMemberRole.owner
        ? PopupMenuButton<_MemberAction>(
            tooltip: 'Member actions',
            onSelected: (_) => onRemove(member),
            itemBuilder: (_) => const [
              PopupMenuItem(value: _MemberAction.remove, child: Text('Remove')),
            ],
          )
        : null,
  );
}

enum _MemberAction { remove }
