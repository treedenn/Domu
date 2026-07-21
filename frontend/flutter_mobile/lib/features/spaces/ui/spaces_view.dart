import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../domain/space.dart';
import 'spaces_view_model.dart';
import 'widgets/item_form_dialog.dart';
import 'widgets/move_space_dialog.dart';
import 'widgets/space_form_dialog.dart';
import 'widgets/space_header.dart';
import 'widgets/space_item_tile.dart';
import 'widgets/space_tile.dart';

class SpacesView extends StatefulWidget {
  const SpacesView({
    super.key,
    required this.householdId,
    required this.viewModel,
    this.spaceId,
  });
  final String householdId;
  final String? spaceId;
  final SpacesViewModel viewModel;

  @override
  State<SpacesView> createState() => _SpacesViewState();
}

class _SpacesViewState extends State<SpacesView> {
  @override
  void initState() {
    super.initState();
    _scheduleLoad();
  }

  @override
  void didUpdateWidget(covariant SpacesView oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.householdId != widget.householdId ||
        oldWidget.spaceId != widget.spaceId) {
      _scheduleLoad();
    }
  }

  void _scheduleLoad() => WidgetsBinding.instance.addPostFrameCallback((_) {
    if (mounted) {
      widget.viewModel.load(widget.householdId, parentId: widget.spaceId);
    }
  });

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
    animation: widget.viewModel,
    builder: (_, _) {
      final vm = widget.viewModel;
      return Scaffold(
        floatingActionButton: FloatingActionButton.extended(
          onPressed: vm.isMutating ? null : _create,
          icon: const Icon(Icons.add),
          label: Text(widget.spaceId == null ? 'New space' : 'Add'),
        ),
        body: Column(
          children: [
            if (vm.message case final message?)
              MaterialBanner(
                content: Text(message),
                actions: [
                  TextButton(
                    onPressed: vm.clearMessage,
                    child: const Text('OK'),
                  ),
                ],
              ),
            if (vm.currentSpace case final space?)
              SpaceHeader(
                space: space,
                path: vm.spacePath,
                onNavigate: _goToSpace,
                onEdit: _editSpace,
                onMove: _moveSpace,
                onDelete: _deleteSpace,
              ),
            Expanded(child: _body(vm)),
          ],
        ),
      );
    },
  );

  Widget _body(SpacesViewModel vm) {
    if (vm.isLoading && vm.spaces.isEmpty && vm.items.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }
    if (vm.errorMessage case final error?) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(error, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () => vm.load(
                widget.householdId,
                parentId: widget.spaceId,
                refresh: true,
              ),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: () =>
          vm.load(widget.householdId, parentId: widget.spaceId, refresh: true),
      child: ListView(
        children: [
          if (vm.isEmpty)
            const Padding(
              padding: EdgeInsets.only(top: 220),
              child: Center(child: Text('No spaces or items yet.')),
            ),
          for (final space in vm.spaces)
            SpaceTile(space: space, onTap: () => _goToSpace(space.id)),
          if (widget.spaceId != null && vm.items.isNotEmpty)
            const Padding(
              padding: EdgeInsets.fromLTRB(16, 16, 16, 4),
              child: Text('Items'),
            ),
          for (final item in vm.items)
            SpaceItemTile(item: item, onEdit: _editItem, onDelete: _deleteItem),
          if (vm.hasMore)
            Padding(
              padding: const EdgeInsets.all(16),
              child: OutlinedButton(
                onPressed: vm.isLoading ? null : vm.loadMore,
                child: const Text('Load more spaces'),
              ),
            ),
        ],
      ),
    );
  }

  void _goToSpace(String? spaceId) => context.go(
    spaceId == null
        ? '/households/${widget.householdId}/spaces'
        : '/households/${widget.householdId}/spaces/$spaceId',
  );

  Future<void> _create() async {
    if (widget.spaceId == null) {
      final value = await _spaceDialog();
      if (value != null) {
        await widget.viewModel.createSpace(
          value.name,
          description: value.description,
        );
      }
      return;
    }
    final choice = await showModalBottomSheet<_AddChoice>(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.folder_outlined),
              title: const Text('New subspace'),
              onTap: () => Navigator.pop(context, _AddChoice.space),
            ),
            ListTile(
              leading: const Icon(Icons.inventory_2_outlined),
              title: const Text('New item'),
              onTap: () => Navigator.pop(context, _AddChoice.item),
            ),
          ],
        ),
      ),
    );
    if (choice == _AddChoice.space) {
      final value = await _spaceDialog();
      if (value != null) {
        await widget.viewModel.createSpace(
          value.name,
          description: value.description,
        );
      }
      return;
    }
    if (choice == _AddChoice.item) {
      final item = await _itemDialog();
      if (item != null) {
        await widget.viewModel.createItem(
          item.name,
          category: item.category,
          barcode: item.barcode,
          entries: item.entries,
        );
      }
    }
  }

  Future<void> _editSpace(Space space) async {
    final value = await _spaceDialog(space: space);
    if (value != null) {
      await widget.viewModel.updateSpace(
        space.id,
        value.name,
        description: value.description,
      );
    }
  }

  Future<void> _moveSpace(Space space) async {
    final destinations = await widget.viewModel.moveDestinations(space.id);
    if (!mounted) return;
    final selection = await showDialog<String>(
      context: context,
      builder: (_) => MoveSpaceDialog(destinations: destinations),
    );
    if (!mounted ||
        selection == null ||
        selection == MoveSpaceDialog.cancelled) {
      return;
    }
    await widget.viewModel.moveSpace(
      space.id,
      selection == MoveSpaceDialog.root ? null : selection,
    );
  }

  Future<void> _deleteSpace(Space space) async {
    if (space.childSpaceCount > 0 || space.itemCount > 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Delete child spaces and items before deleting this space.',
          ),
        ),
      );
      return;
    }
    final yes = await _confirm('Delete space?', 'Delete ${space.name}?');
    if (yes == true && mounted) {
      final ok = await widget.viewModel.deleteSpace(space.id);
      if (ok && mounted) context.pop();
    }
  }

  Future<void> _editItem(SpaceItem item) async {
    final value = await _itemDialog(item: item);
    if (value != null) {
      await widget.viewModel.updateItem(
        item,
        value.name,
        category: value.category,
        barcode: value.barcode,
        entries: value.entries,
      );
    }
  }

  Future<void> _deleteItem(SpaceItem item) async {
    final yes = await _confirm('Delete item?', 'Delete ${item.name}?');
    if (yes == true) {
      await widget.viewModel.deleteItem(item.id);
    }
  }

  Future<bool?> _confirm(String title, String content) => showDialog<bool>(
    context: context,
    builder: (_) => AlertDialog(
      title: Text(title),
      content: Text(content),
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
  Future<SpaceFormValues?> _spaceDialog({Space? space}) =>
      showDialog<SpaceFormValues>(
        context: context,
        builder: (_) => SpaceFormDialog(space: space),
      );
  Future<ItemFormValues?> _itemDialog({SpaceItem? item}) =>
      showDialog<ItemFormValues>(
        context: context,
        builder: (_) => ItemFormDialog(item: item),
      );
}

enum _AddChoice { space, item }
