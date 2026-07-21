import 'package:flutter/material.dart';

import '../../domain/shopping_list.dart';

class ShoppingListItemTile extends StatelessWidget {
  const ShoppingListItemTile({
    super.key,
    required this.item,
    required this.isMutating,
    required this.onToggle,
    required this.onEdit,
    required this.onDelete,
  });

  final ShoppingListItem item;
  final bool isMutating;
  final ValueChanged<ShoppingListItem> onToggle;
  final ValueChanged<ShoppingListItem> onEdit;
  final ValueChanged<ShoppingListItem> onDelete;

  @override
  Widget build(BuildContext context) => ListTile(
    key: ValueKey('shopping-item-${item.id}'),
    leading: Checkbox(
      value: item.checked,
      onChanged: isMutating ? null : (_) => onToggle(item),
    ),
    title: Text(item.name, style: _style),
    subtitle: item.note == null || item.note!.isEmpty
        ? null
        : Text(item.note!, style: _style),
    trailing: PopupMenuButton<_ItemAction>(
      tooltip: 'Item actions',
      enabled: !isMutating,
      onSelected: (action) =>
          action == _ItemAction.edit ? onEdit(item) : onDelete(item),
      itemBuilder: (_) => const [
        PopupMenuItem(value: _ItemAction.edit, child: Text('Edit')),
        PopupMenuItem(value: _ItemAction.delete, child: Text('Delete')),
      ],
    ),
  );

  TextStyle? get _style => item.checked
      ? const TextStyle(decoration: TextDecoration.lineThrough)
      : null;
}

enum _ItemAction { edit, delete }
