import 'package:flutter/material.dart';

import '../../domain/household.dart';

class HouseholdTile extends StatelessWidget {
  const HouseholdTile({
    super.key,
    required this.household,
    required this.selected,
    required this.onSelected,
    required this.onRename,
    required this.onDelete,
  });

  final Household household;
  final bool selected;
  final ValueChanged<Household> onSelected;
  final ValueChanged<Household> onRename;
  final ValueChanged<Household> onDelete;

  @override
  Widget build(BuildContext context) => ListTile(
    key: ValueKey('household-${household.id}'),
    selected: selected,
    leading: Icon(selected ? Icons.check_circle : Icons.home_outlined),
    title: Text(household.name),
    subtitle: selected ? const Text('Selected for this session') : null,
    onTap: () => onSelected(household),
    trailing: PopupMenuButton<_HouseholdAction>(
      tooltip: 'Household actions',
      onSelected: (action) => action == _HouseholdAction.rename
          ? onRename(household)
          : onDelete(household),
      itemBuilder: (_) => const [
        PopupMenuItem(value: _HouseholdAction.rename, child: Text('Rename')),
        PopupMenuItem(value: _HouseholdAction.delete, child: Text('Delete')),
      ],
    ),
  );
}

enum _HouseholdAction { rename, delete }
