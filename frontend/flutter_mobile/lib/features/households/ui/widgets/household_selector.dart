import 'package:flutter/material.dart';

import '../../domain/household.dart';

class HouseholdSelector extends StatelessWidget {
  const HouseholdSelector({
    super.key,
    required this.household,
    required this.households,
    required this.onSelected,
    required this.onManage,
  });

  final Household household;
  final List<Household> households;
  final ValueChanged<Household> onSelected;
  final VoidCallback onManage;

  @override
  Widget build(BuildContext context) => PopupMenuButton<Household?>(
    tooltip: 'Select household',
    onSelected: (selected) =>
        selected == null ? onManage() : onSelected(selected),
    itemBuilder: (context) => [
      ...households.map(
        (candidate) =>
            PopupMenuItem(value: candidate, child: Text(candidate.name)),
      ),
      const PopupMenuDivider(),
      const PopupMenuItem<Household?>(
        value: null,
        child: Text('Manage households'),
      ),
    ],
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [Text(household.name), const Icon(Icons.arrow_drop_down)],
    ),
  );
}
