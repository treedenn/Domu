import 'package:flutter/material.dart';
import '../domain/shopping_list.dart';
import 'shopping_list_detail_view_model.dart';
import 'widgets/shopping_list_item_tile.dart';

class ShoppingListDetailView extends StatefulWidget {
  const ShoppingListDetailView({
    super.key,
    required this.householdId,
    required this.shoppingListId,
    required this.viewModel,
  });
  final String householdId;
  final String shoppingListId;
  final ShoppingListDetailViewModel viewModel;
  @override
  State<ShoppingListDetailView> createState() => _ShoppingListDetailViewState();
}

class _ShoppingListDetailViewState extends State<ShoppingListDetailView> {
  final _controller = TextEditingController();
  final _noteController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  @override
  void initState() {
    super.initState();
    widget.viewModel.load(widget.householdId, widget.shoppingListId);
  }

  @override
  void didUpdateWidget(covariant ShoppingListDetailView old) {
    super.didUpdateWidget(old);
    if (old.householdId != widget.householdId ||
        old.shoppingListId != widget.shoppingListId) {
      widget.viewModel.load(widget.householdId, widget.shoppingListId);
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    _noteController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
    animation: widget.viewModel,
    builder: (context, _) => Scaffold(
      body: Column(
        children: [
          if (widget.viewModel.message case final message?)
            MaterialBanner(
              content: Text(message),
              actions: [
                TextButton(
                  onPressed: widget.viewModel.clearMessage,
                  child: const Text('OK'),
                ),
              ],
            ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Form(
              key: _formKey,
              child: Column(
                children: [
                  TextFormField(
                    controller: _controller,
                    maxLength: 120,
                    decoration: const InputDecoration(labelText: 'Add item'),
                    validator: _validate,
                  ),
                  const SizedBox(width: 8),
                  TextFormField(
                    controller: _noteController,
                    maxLength: 500,
                    minLines: 1,
                    maxLines: 3,
                    decoration: const InputDecoration(
                      labelText: 'Note (optional)',
                      hintText: 'For example, why this is needed',
                    ),
                    validator: _validateNote,
                  ),
                  const SizedBox(height: 8),
                  Align(
                    alignment: Alignment.centerRight,
                    child: FilledButton(
                      onPressed: widget.viewModel.isMutating ? null : _add,
                      child: const Text('Add'),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (widget.viewModel.hasCompleted)
            Align(
              alignment: Alignment.centerRight,
              child: TextButton(
                onPressed: widget.viewModel.isMutating ? null : _clear,
                child: const Text('Clear completed'),
              ),
            ),
          Expanded(child: _body()),
        ],
      ),
    ),
  );
  Widget _body() {
    final vm = widget.viewModel;
    if (vm.isLoading) return const Center(child: CircularProgressIndicator());
    if (vm.errorMessage case final error?) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(error),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () =>
                  vm.load(widget.householdId, widget.shoppingListId),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }
    if (vm.isEmpty) {
      return RefreshIndicator(
        onRefresh: vm.refresh,
        child: ListView(
          children: const [
            SizedBox(height: 180),
            Center(child: Text('No items yet.')),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: vm.refresh,
      child: ListView(
        children: [
          for (final item in vm.uncheckedItems)
            ShoppingListItemTile(
              item: item,
              isMutating: vm.isMutating,
              onToggle: vm.toggle,
              onEdit: _edit,
              onDelete: _delete,
            ),
          if (vm.completedItems.isNotEmpty)
            const Padding(
              padding: EdgeInsets.fromLTRB(16, 16, 16, 4),
              child: Text('Completed'),
            ),
          for (final item in vm.completedItems)
            ShoppingListItemTile(
              item: item,
              isMutating: vm.isMutating,
              onToggle: vm.toggle,
              onEdit: _edit,
              onDelete: _delete,
            ),
        ],
      ),
    );
  }

  Future<void> _add() async {
    if (!_formKey.currentState!.validate()) return;
    final name = _controller.text.trim();
    final note = _noteController.text.trim();
    final ok = await widget.viewModel.add(
      name,
      note: note.isEmpty ? null : note,
    );
    if (ok && mounted) {
      _controller.clear();
      _noteController.clear();
    }
  }

  Future<void> _delete(ShoppingListItem item) async {
    final yes = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete item?'),
        content: Text('Delete ${item.name}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (yes == true) await widget.viewModel.delete(item);
  }

  Future<void> _edit(ShoppingListItem item) async {
    final result = await showDialog<_ItemEdit>(
      context: context,
      builder: (_) => _EditItemDialog(item: item),
    );
    if (result != null) {
      await widget.viewModel.update(item, result.name, note: result.note);
    }
  }

  Future<void> _clear() async {
    final yes = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Clear completed items?'),
        content: const Text('Remove all completed items from this list?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Clear'),
          ),
        ],
      ),
    );
    if (yes == true) await widget.viewModel.clearCompleted();
  }

  String? _validate(String? value) {
    final name = value?.trim() ?? '';
    if (name.isEmpty) return 'Required';
    if (name.length > 120) return 'Use 120 characters or fewer';
    return null;
  }

  String? _validateNote(String? value) {
    if ((value?.trim().length ?? 0) > 500) {
      return 'Use 500 characters or fewer';
    }
    return null;
  }
}

class _ItemEdit {
  const _ItemEdit({required this.name, required this.note});

  final String name;
  final String? note;
}

class _EditItemDialog extends StatefulWidget {
  const _EditItemDialog({required this.item});

  final ShoppingListItem item;

  @override
  State<_EditItemDialog> createState() => _EditItemDialogState();
}

class _EditItemDialogState extends State<_EditItemDialog> {
  late final _nameController = TextEditingController(text: widget.item.name);
  late final _noteController = TextEditingController(
    text: widget.item.note ?? '',
  );
  final _formKey = GlobalKey<FormState>();

  @override
  void dispose() {
    _nameController.dispose();
    _noteController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: const Text('Edit item'),
    content: Form(
      key: _formKey,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: _nameController,
            autofocus: true,
            maxLength: 120,
            decoration: const InputDecoration(labelText: 'Item name'),
            validator: _nameValidator,
          ),
          TextFormField(
            controller: _noteController,
            maxLength: 500,
            minLines: 1,
            maxLines: 3,
            decoration: const InputDecoration(labelText: 'Note (optional)'),
            validator: _noteValidator,
          ),
        ],
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: () {
          if (!_formKey.currentState!.validate()) return;
          final note = _noteController.text.trim();
          Navigator.pop(
            context,
            _ItemEdit(
              name: _nameController.text.trim(),
              note: note.isEmpty ? null : note,
            ),
          );
        },
        child: const Text('Save'),
      ),
    ],
  );
}

String? _nameValidator(String? value) {
  final name = value?.trim() ?? '';
  if (name.isEmpty) return 'Required';
  if (name.length > 120) return 'Use 120 characters or fewer';
  return null;
}

String? _noteValidator(String? value) {
  if ((value?.trim().length ?? 0) > 500) {
    return 'Use 500 characters or fewer';
  }
  return null;
}
