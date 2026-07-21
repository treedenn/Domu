import 'package:flutter/material.dart';

import '../../domain/shopping_list.dart';

class ShoppingListTile extends StatelessWidget {
  const ShoppingListTile({
    super.key,
    required this.list,
    required this.onTap,
    required this.onRename,
    required this.onDelete,
  });

  final ShoppingList list;
  final VoidCallback onTap;
  final ValueChanged<ShoppingList> onRename;
  final ValueChanged<ShoppingList> onDelete;

  @override
  Widget build(BuildContext context) => ListTile(
    key: ValueKey('shopping-list-${list.id}'),
    leading: const Icon(Icons.shopping_cart_outlined),
    title: Text(list.name),
    trailing: PopupMenuButton<_ListAction>(
      tooltip: 'List actions',
      onSelected: (action) =>
          action == _ListAction.rename ? onRename(list) : onDelete(list),
      itemBuilder: (_) => const [
        PopupMenuItem(value: _ListAction.rename, child: Text('Rename')),
        PopupMenuItem(value: _ListAction.delete, child: Text('Delete list')),
      ],
    ),
    onTap: onTap,
  );
}

enum _ListAction { rename, delete }
