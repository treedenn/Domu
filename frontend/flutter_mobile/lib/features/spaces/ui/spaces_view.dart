import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../domain/space.dart';
import '../domain/item_unit_converter.dart';
import 'spaces_view_model.dart';

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
  void didUpdateWidget(covariant SpacesView old) {
    super.didUpdateWidget(old);
    if (old.householdId != widget.householdId ||
        old.spaceId != widget.spaceId) {
      _scheduleLoad();
    }
  }

  void _scheduleLoad() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        widget.viewModel.load(widget.householdId, parentId: widget.spaceId);
      }
    });
  }

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
            if (vm.currentSpace case final space?) _header(space, vm.spacePath),
            Expanded(child: _body(vm)),
          ],
        ),
      );
    },
  );
  Widget _header(Space space, List<Space> path) {
    final parent = path.length > 1 ? path[path.length - 2] : null;
    final isMainSpace = parent == null;
    return Material(
      color: Theme.of(context).colorScheme.surfaceContainerLow,
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            IconButton(
              tooltip: parent == null
                  ? 'Back to Spaces'
                  : 'Back to ${parent.name}',
              onPressed: () => _goToSpace(parent?.id),
              icon: const Icon(Icons.arrow_back),
            ),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (path.length > 1) ...[
                    _spacePath(path.take(path.length - 1).toList()),
                    const SizedBox(height: 4),
                  ],
                  Text(
                    isMainSpace ? 'Main space' : 'Subspace',
                    style: Theme.of(context).textTheme.labelLarge,
                  ),
                  Text(
                    space.name,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  if (space.description case final description?
                      when description.isNotEmpty)
                    Text(description),
                ],
              ),
            ),
            PopupMenuButton<_SpaceAction>(
              onSelected: (action) => switch (action) {
                _SpaceAction.edit => _editSpace(space),
                _SpaceAction.move => _moveSpace(space),
                _SpaceAction.delete => _deleteSpace(space),
              },
              itemBuilder: (_) => const [
                PopupMenuItem(
                  value: _SpaceAction.edit,
                  child: Text('Edit space'),
                ),
                PopupMenuItem(
                  value: _SpaceAction.move,
                  child: Text('Move space'),
                ),
                PopupMenuItem(
                  value: _SpaceAction.delete,
                  child: Text('Delete space'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _spacePath(List<Space> path) => Wrap(
    crossAxisAlignment: WrapCrossAlignment.center,
    spacing: 2,
    children: [
      for (var index = 0; index < path.length; index++) ...[
        if (index > 0) const Icon(Icons.chevron_right, size: 18),
        TextButton(
          onPressed: () => _goToSpace(path[index].id),
          child: Text(path[index].name),
        ),
      ],
    ],
  );

  void _goToSpace(String? spaceId) => context.go(
    spaceId == null
        ? '/households/${widget.householdId}/spaces'
        : '/households/${widget.householdId}/spaces/$spaceId',
  );
  Widget _body(SpacesViewModel vm) {
    if (vm.isLoading && vm.spaces.isEmpty && vm.items.isEmpty)
      return const Center(child: CircularProgressIndicator());
    if (vm.errorMessage case final error?)
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
          for (final space in vm.spaces) _spaceTile(space),
          if (widget.spaceId != null && vm.items.isNotEmpty)
            const Padding(
              padding: EdgeInsets.fromLTRB(16, 16, 16, 4),
              child: Text('Items'),
            ),
          for (final item in vm.items) _itemTile(item),
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

  Widget _spaceTile(Space space) => ListTile(
    leading: const Icon(Icons.folder_outlined),
    title: Text(space.name),
    subtitle: Text(
      '${space.childSpaceCount} subspaces · ${space.itemCount} items',
    ),
    trailing: const Icon(Icons.chevron_right),
    onTap: () =>
        context.go('/households/${widget.householdId}/spaces/${space.id}'),
  );
  Widget _itemTile(SpaceItem item) => ListTile(
    leading: const Icon(Icons.inventory_2_outlined),
    title: Text(item.name),
    subtitle: Text(_itemSummary(item)),
    trailing: PopupMenuButton<_ItemAction>(
      onSelected: (action) =>
          action == _ItemAction.edit ? _editItem(item) : _deleteItem(item),
      itemBuilder: (_) => const [
        PopupMenuItem(value: _ItemAction.edit, child: Text('Edit')),
        PopupMenuItem(value: _ItemAction.delete, child: Text('Delete')),
      ],
    ),
    onTap: () => _editItem(item),
  );
  String _itemSummary(SpaceItem item) => [
    if (item.category?.isNotEmpty == true) item.category!,
    '${item.totalCount} total',
    '${item.entries.length} entries',
  ].join(' · ');
  Future<void> _create() async {
    if (widget.spaceId == null) {
      final value = await _spaceDialog();
      if (value != null)
        await widget.viewModel.createSpace(
          value.name,
          description: value.description,
        );
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
    if (choice != _AddChoice.item) return;
    final item = await _itemDialog();
    if (item != null)
      await widget.viewModel.createItem(
        item.name,
        category: item.category,
        barcode: item.barcode,
        entries: item.entries,
      );
  }

  Future<void> _editSpace(Space space) async {
    final value = await _spaceDialog(space: space);
    if (value != null)
      await widget.viewModel.updateSpace(
        space.id,
        value.name,
        description: value.description,
      );
  }

  Future<void> _moveSpace(Space space) async {
    final destinations = await widget.viewModel.moveDestinations(space.id);
    if (!mounted) return;
    final selection = await showDialog<String>(
      context: context,
      builder: (_) => _MoveSpaceDialog(destinations: destinations),
    );
    if (!mounted ||
        selection == null ||
        selection == _MoveSpaceDialog.cancelled)
      return;
    await widget.viewModel.moveSpace(
      space.id,
      selection == _MoveSpaceDialog.root ? null : selection,
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
    if (value != null)
      await widget.viewModel.updateItem(
        item,
        value.name,
        category: value.category,
        barcode: value.barcode,
        entries: value.entries,
      );
  }

  Future<void> _deleteItem(SpaceItem item) async {
    final yes = await _confirm('Delete item?', 'Delete ${item.name}?');
    if (yes == true) await widget.viewModel.deleteItem(item.id);
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
  Future<_SpaceForm?> _spaceDialog({Space? space}) => showDialog<_SpaceForm>(
    context: context,
    builder: (_) => _SpaceDialog(space: space),
  );
  Future<_ItemForm?> _itemDialog({SpaceItem? item}) => showDialog<_ItemForm>(
    context: context,
    builder: (_) => _ItemDialog(item: item),
  );
}

enum _SpaceAction { edit, move, delete }

enum _ItemAction { edit, delete }

enum _AddChoice { space, item }

class _SpaceForm {
  const _SpaceForm(this.name, this.description);
  final String name;
  final String? description;
}

class _SpaceDialog extends StatefulWidget {
  const _SpaceDialog({this.space});
  final Space? space;
  @override
  State<_SpaceDialog> createState() => _SpaceDialogState();
}

class _SpaceDialogState extends State<_SpaceDialog> {
  late final name = TextEditingController(text: widget.space?.name);
  late final description = TextEditingController(
    text: widget.space?.description,
  );
  final key = GlobalKey<FormState>();
  @override
  void dispose() {
    name.dispose();
    description.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.space == null ? 'New space' : 'Edit space'),
    content: Form(
      key: key,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: name,
            autofocus: true,
            maxLength: 100,
            decoration: const InputDecoration(labelText: 'Name'),
            validator: _required,
          ),
          TextFormField(
            controller: description,
            maxLength: 255,
            minLines: 1,
            maxLines: 3,
            decoration: const InputDecoration(
              labelText: 'Description (optional)',
            ),
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
          if (key.currentState!.validate())
            Navigator.pop(
              context,
              _SpaceForm(name.text.trim(), _optional(description.text)),
            );
        },
        child: const Text('Save'),
      ),
    ],
  );
}

class _ItemForm {
  const _ItemForm(this.name, this.category, this.barcode, this.entries);
  final String name;
  final String? category;
  final String? barcode;
  final List<ItemEntry> entries;
}

class _ItemDialog extends StatefulWidget {
  const _ItemDialog({this.item});
  final SpaceItem? item;
  @override
  State<_ItemDialog> createState() => _ItemDialogState();
}

class _MoveSpaceDialog extends StatelessWidget {
  const _MoveSpaceDialog({required this.destinations});
  static const cancelled = '__cancelled__';
  static const root = '__root__';
  final List<Space> destinations;
  @override
  Widget build(BuildContext context) => SimpleDialog(
    title: const Text('Move space to'),
    children: [
      SimpleDialogOption(
        onPressed: () => Navigator.pop(context, root),
        child: const Text('Top level'),
      ),
      ...destinations.map(
        (space) => SimpleDialogOption(
          onPressed: () => Navigator.pop(context, space.id),
          child: Text(space.name),
        ),
      ),
      SimpleDialogOption(
        onPressed: () => Navigator.pop(context, cancelled),
        child: const Text('Cancel'),
      ),
    ],
  );
}

class _ItemDialogState extends State<_ItemDialog> {
  late final name = TextEditingController(text: widget.item?.name);
  late final category = TextEditingController(text: widget.item?.category);
  late final barcode = TextEditingController(text: widget.item?.barcode);
  late final entries = [...?widget.item?.entries];
  final key = GlobalKey<FormState>();
  @override
  void dispose() {
    name.dispose();
    category.dispose();
    barcode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.item == null ? 'New item' : 'Edit item'),
    content: SizedBox(
      width: 360,
      child: Form(
        key: key,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: name,
                autofocus: true,
                maxLength: 255,
                decoration: const InputDecoration(labelText: 'Name'),
                validator: _required,
              ),
              TextFormField(
                controller: category,
                maxLength: 255,
                decoration: const InputDecoration(
                  labelText: 'Category (optional)',
                ),
              ),
              TextFormField(
                controller: barcode,
                maxLength: 128,
                decoration: const InputDecoration(
                  labelText: 'Barcode (optional)',
                ),
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  const Text('Stock entries'),
                  const Spacer(),
                  TextButton.icon(
                    onPressed: _addEntry,
                    icon: const Icon(Icons.add),
                    label: const Text('Add'),
                  ),
                ],
              ),
              for (var i = 0; i < entries.length; i++)
                ListTile(
                  title: Text(
                    entries[i].state == ConsumableState.unopened
                        ? '${entries[i].count} × ${entries[i].originalAmountPerUnit ?? '-'} ${entries[i].unit.name}'
                        : '${entries[i].count} × ${entries[i].currentAmountPerUnit ?? '-'} / ${entries[i].originalAmountPerUnit ?? '-'} ${entries[i].unit.name}',
                  ),
                  subtitle: Text(entries[i].state.name),
                  onTap: () => _editEntry(i),
                  trailing: IconButton(
                    icon: const Icon(Icons.delete_outline),
                    onPressed: () => setState(() => entries.removeAt(i)),
                  ),
                ),
            ],
          ),
        ),
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: () {
          if (key.currentState!.validate())
            Navigator.pop(
              context,
              _ItemForm(
                name.text.trim(),
                _optional(category.text),
                _optional(barcode.text),
                entries,
              ),
            );
        },
        child: const Text('Save'),
      ),
    ],
  );
  Future<void> _addEntry() async {
    final entry = await showDialog<ItemEntry>(
      context: context,
      builder: (_) => const _EntryDialog(),
    );
    if (entry != null) setState(() => entries.add(entry));
  }

  Future<void> _editEntry(int index) async {
    final entry = await showDialog<ItemEntry>(
      context: context,
      builder: (_) => _EntryDialog(entry: entries[index]),
    );
    if (entry != null) setState(() => entries[index] = entry);
  }
}

class _EntryDialog extends StatefulWidget {
  const _EntryDialog({this.entry});

  final ItemEntry? entry;
  @override
  State<_EntryDialog> createState() => _EntryDialogState();
}

class _EntryDialogState extends State<_EntryDialog> {
  late final count = TextEditingController(text: '${widget.entry?.count ?? 1}');
  late final original = TextEditingController(
    text: '${widget.entry?.originalAmountPerUnit ?? 1}',
  );
  late final current = TextEditingController(
    text: '${widget.entry?.currentAmountPerUnit ?? 1}',
  );
  late ItemUnit unit = widget.entry?.unit ?? ItemUnit.piece;
  late ItemUnit? _lastConvertibleUnit = unit == ItemUnit.unspecified
      ? null
      : unit;
  late ConsumableState state =
      widget.entry?.state ?? ConsumableState.unspecified;
  DateTime? acquisitionDate;
  DateTime? expirationDate;
  final key = GlobalKey<FormState>();
  @override
  void dispose() {
    count.dispose();
    original.dispose();
    current.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.entry == null ? 'New stock entry' : 'Edit stock entry'),
    content: Form(
      key: key,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: count,
            keyboardType: TextInputType.number,
            decoration: const InputDecoration(labelText: 'Count'),
            validator: _count,
          ),
          TextFormField(
            controller: original,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            decoration: const InputDecoration(
              labelText: 'Original amount per unit',
            ),
            validator: _quantity,
            onChanged: (value) {
              if (state == ConsumableState.unopened) current.text = value;
            },
          ),
          if (state != ConsumableState.unopened)
            TextFormField(
              controller: current,
              keyboardType: const TextInputType.numberWithOptions(
                decimal: true,
              ),
              decoration: const InputDecoration(
                labelText: 'Current amount per unit',
              ),
              validator: _quantity,
            ),
          DropdownButtonFormField(
            value: unit,
            items: ItemUnit.values
                .map((e) => DropdownMenuItem(value: e, child: Text(e.name)))
                .toList(),
            onChanged: (value) => _changeUnit(value!),
          ),
          DropdownButtonFormField(
            value: state,
            items: ConsumableState.values
                .map((e) => DropdownMenuItem(value: e, child: Text(e.name)))
                .toList(),
            onChanged: (value) => setState(() {
              state = value!;
              if (state == ConsumableState.unopened) {
                current.text = original.text;
              }
            }),
          ),
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: const Text('Acquisition date'),
            subtitle: Text(
              acquisitionDate?.toIso8601String().split('T').first ?? 'Not set',
            ),
            trailing: const Icon(Icons.calendar_today_outlined),
            onTap: () => _pickDate(acquisition: true),
          ),
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: const Text('Expiration date'),
            subtitle: Text(
              expirationDate?.toIso8601String().split('T').first ?? 'Not set',
            ),
            trailing: const Icon(Icons.calendar_today_outlined),
            onTap: () => _pickDate(acquisition: false),
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
          if (!key.currentState!.validate()) return;
          final entryCount = int.parse(count.text);
          final originalAmountPerUnit = num.parse(original.text);
          final c = state == ConsumableState.unopened
              ? originalAmountPerUnit
              : num.parse(current.text);
          if (c > originalAmountPerUnit) return;
          Navigator.pop(
            context,
            ItemEntry(
              id: widget.entry?.id,
              count: entryCount,
              originalAmountPerUnit: originalAmountPerUnit,
              currentAmountPerUnit: c,
              unit: unit,
              state: state,
              acquisitionDate: acquisitionDate,
              expirationDate: expirationDate,
            ),
          );
        },
        child: Text(widget.entry == null ? 'Add' : 'Save'),
      ),
    ],
  );

  Future<void> _pickDate({required bool acquisition}) async {
    final initialDate = acquisition ? acquisitionDate : expirationDate;
    final selected = await showDatePicker(
      context: context,
      initialDate: initialDate ?? DateTime.now(),
      firstDate: DateTime(2000),
      lastDate: DateTime(2100),
    );
    if (selected == null) return;
    setState(() {
      if (acquisition) {
        acquisitionDate = selected;
      } else {
        expirationDate = selected;
      }
    });
  }

  void _changeUnit(ItemUnit nextUnit) {
    if (nextUnit == unit) return;

    if (nextUnit == ItemUnit.unspecified) {
      setState(() => unit = nextUnit);
      return;
    }

    final originalAmount = num.tryParse(original.text);
    final currentAmount = num.tryParse(current.text);
    setState(() {
      final fromUnit = _lastConvertibleUnit;
      if (originalAmount != null && fromUnit != null) {
        original.text = ItemUnitConverter.convert(
          originalAmount,
          from: fromUnit,
          to: nextUnit,
        ).toString();
      }
      if (state == ConsumableState.unopened) {
        current.text = original.text;
      } else if (currentAmount != null && fromUnit != null) {
        current.text = ItemUnitConverter.convert(
          currentAmount,
          from: fromUnit,
          to: nextUnit,
        ).toString();
      }
      unit = nextUnit;
      _lastConvertibleUnit = nextUnit;
    });
  }
}

String? _required(String? value) =>
    value?.trim().isEmpty ?? true ? 'Required' : null;
String? _quantity(String? value) {
  final parsed = num.tryParse(value ?? '');
  return parsed == null || parsed < 0 ? 'Enter a non-negative number' : null;
}

String? _count(String? value) {
  final count = int.tryParse(value ?? '');
  if (count == null || count < 1) return 'Enter a whole number of at least 1';
  return null;
}

String? _optional(String value) => value.trim().isEmpty ? null : value.trim();
