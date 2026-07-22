import 'package:flutter/material.dart';

import '../../domain/space.dart';

class SpaceItemTile extends StatelessWidget {
  const SpaceItemTile({
    super.key,
    required this.item,
    required this.onEdit,
    required this.onDelete,
    required this.onAddToShoppingList,
  });

  final SpaceItem item;
  final ValueChanged<SpaceItem> onEdit;
  final ValueChanged<SpaceItem> onDelete;
  final ValueChanged<SpaceItem> onAddToShoppingList;

  @override
  Widget build(BuildContext context) => ListTile(
    leading: const Icon(Icons.inventory_2_outlined),
    title: Text(item.name),
    subtitle: Text(
      [
        if (item.category?.isNotEmpty == true) item.category!,
        '${item.totalCount} total',
        '${item.entries.length} entries',
      ].join(' · '),
    ),
    trailing: PopupMenuButton<_ItemAction>(
      onSelected: (action) => switch (action) {
        _ItemAction.edit => onEdit(item),
        _ItemAction.addToShoppingList => onAddToShoppingList(item),
        _ItemAction.delete => onDelete(item),
      },
      itemBuilder: (_) => const [
        PopupMenuItem(value: _ItemAction.edit, child: Text('Edit')),
        PopupMenuItem(
          value: _ItemAction.addToShoppingList,
          child: Text('Add to shopping list'),
        ),
        PopupMenuItem(value: _ItemAction.delete, child: Text('Delete')),
      ],
    ),
    onTap: () => onEdit(item),
  );
}

enum _ItemAction { edit, addToShoppingList, delete }
