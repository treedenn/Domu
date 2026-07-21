import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../domain/shopping_list.dart';
import 'shopping_lists_view_model.dart';
import 'widgets/shopping_list_tile.dart';

class ShoppingListsView extends StatefulWidget {
  const ShoppingListsView({
    super.key,
    required this.householdId,
    required this.viewModel,
  });
  final String householdId;
  final ShoppingListsViewModel viewModel;
  @override
  State<ShoppingListsView> createState() => _ShoppingListsViewState();
}

class _ShoppingListsViewState extends State<ShoppingListsView> {
  @override
  void initState() {
    super.initState();
    widget.viewModel.load(widget.householdId);
  }

  @override
  void didUpdateWidget(covariant ShoppingListsView old) {
    super.didUpdateWidget(old);
    if (old.householdId != widget.householdId) {
      widget.viewModel.load(widget.householdId);
    }
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
    animation: widget.viewModel,
    builder: (context, _) => Scaffold(
      floatingActionButton: FloatingActionButton.extended(
        onPressed: widget.viewModel.isMutating ? null : _create,
        icon: const Icon(Icons.add),
        label: const Text('New list'),
      ),
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
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(error, textAlign: TextAlign.center),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => vm.load(widget.householdId),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: vm.refresh,
      child: vm.isEmpty
          ? ListView(
              children: const [
                SizedBox(height: 240),
                Center(
                  child: Text(
                    'No shopping lists yet. Create one to get started.',
                  ),
                ),
              ],
            )
          : ListView(
              children: vm.lists
                  .map(
                    (list) => ShoppingListTile(
                      list: list,
                      onTap: () => context.go(
                        '/households/${widget.householdId}/shopping-lists/${list.id}',
                      ),
                      onRename: _rename,
                      onDelete: _archive,
                    ),
                  )
                  .toList(),
            ),
    );
  }

  Future<void> _create() async {
    final name = await _nameDialog(
      title: 'New shopping list',
      action: 'Create',
    );
    if (name != null) await widget.viewModel.create(name);
  }

  Future<void> _rename(ShoppingList list) async {
    final name = await _nameDialog(
      title: 'Rename shopping list',
      action: 'Save',
      initial: list.name,
    );
    if (name != null) await widget.viewModel.rename(list, name);
  }

  Future<void> _archive(ShoppingList list) async {
    final yes = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete list?'),
        content: Text('Delete ${list.name}?'),
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
    if (yes == true) await widget.viewModel.archive(list);
  }

  Future<String?> _nameDialog({
    required String title,
    required String action,
    String initial = '',
  }) => showDialog<String>(
    context: context,
    builder: (_) => _ListNameDialog(
      title: title,
      action: action,
      initial: initial,
      validator: _nameValidator,
    ),
  );

  String? _nameValidator(String? value) {
    final name = value?.trim() ?? '';
    if (name.isEmpty) return 'Required';
    if (name.length > 120) return 'Use 120 characters or fewer';
    return null;
  }
}

class _ListNameDialog extends StatefulWidget {
  const _ListNameDialog({
    required this.title,
    required this.action,
    required this.initial,
    required this.validator,
  });

  final String title;
  final String action;
  final String initial;
  final String? Function(String?) validator;

  @override
  State<_ListNameDialog> createState() => _ListNameDialogState();
}

class _ListNameDialogState extends State<_ListNameDialog> {
  late final controller = TextEditingController(text: widget.initial);
  final key = GlobalKey<FormState>();

  @override
  void dispose() {
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.title),
    content: Form(
      key: key,
      child: TextFormField(
        controller: controller,
        autofocus: true,
        maxLength: 120,
        decoration: const InputDecoration(labelText: 'Name'),
        validator: widget.validator,
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: () {
          if (key.currentState!.validate()) {
            Navigator.pop(context, controller.text.trim());
          }
        },
        child: Text(widget.action),
      ),
    ],
  );
}
