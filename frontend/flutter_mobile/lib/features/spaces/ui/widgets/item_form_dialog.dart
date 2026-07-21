import 'package:flutter/material.dart';

import '../../domain/item_unit_converter.dart';
import '../../domain/space.dart';

class ItemFormValues {
  const ItemFormValues(this.name, this.category, this.barcode, this.entries);
  final String name;
  final String? category;
  final String? barcode;
  final List<ItemEntry> entries;
}

class ItemFormDialog extends StatefulWidget {
  const ItemFormDialog({super.key, this.item});
  final SpaceItem? item;

  @override
  State<ItemFormDialog> createState() => _ItemFormDialogState();
}

class _ItemFormDialogState extends State<ItemFormDialog> {
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
              ItemEntriesEditor(
                entries: entries,
                onChanged: (nextEntries) => setState(() {
                  entries
                    ..clear()
                    ..addAll(nextEntries);
                }),
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
          if (key.currentState!.validate()) {
            Navigator.pop(
              context,
              ItemFormValues(
                name.text.trim(),
                _optional(category.text),
                _optional(barcode.text),
                entries,
              ),
            );
          }
        },
        child: const Text('Save'),
      ),
    ],
  );
}

class ItemEntriesEditor extends StatelessWidget {
  const ItemEntriesEditor({
    super.key,
    required this.entries,
    required this.onChanged,
  });
  final List<ItemEntry> entries;
  final ValueChanged<List<ItemEntry>> onChanged;

  Future<void> _edit(BuildContext context, [int? index]) async {
    final entry = await showDialog<ItemEntry>(
      context: context,
      builder: (_) =>
          EntryFormDialog(entry: index == null ? null : entries[index]),
    );
    if (entry == null) return;
    final next = [...entries];
    if (index == null) {
      next.add(entry);
    } else {
      next[index] = entry;
    }
    onChanged(next);
  }

  @override
  Widget build(BuildContext context) => Column(
    children: [
      Row(
        children: [
          const Text('Stock entries'),
          const Spacer(),
          TextButton.icon(
            onPressed: () => _edit(context),
            icon: const Icon(Icons.add),
            label: const Text('Add'),
          ),
        ],
      ),
      for (var i = 0; i < entries.length; i++)
        ListTile(
          title: Text(_summary(entries[i])),
          subtitle: entries[i].state == ConsumableState.unspecified
              ? null
              : Text(entries[i].state.name),
          onTap: () => _edit(context, i),
          trailing: IconButton(
            icon: const Icon(Icons.delete_outline),
            onPressed: () {
              final next = [...entries]..removeAt(i);
              onChanged(next);
            },
          ),
        ),
    ],
  );

  String _summary(ItemEntry entry) {
    if (entry.unit == ItemUnit.unspecified &&
        entry.originalAmountPerUnit == null &&
        entry.currentAmountPerUnit == null) {
      return '${entry.count} item${entry.count == 1 ? '' : 's'}';
    }
    if (entry.state == ConsumableState.unopened) {
      return '${entry.count} × ${entry.originalAmountPerUnit ?? '-'} ${entry.unit.name}';
    }
    return '${entry.count} × ${entry.currentAmountPerUnit ?? '-'} / ${entry.originalAmountPerUnit ?? '-'} ${entry.unit.name}';
  }
}

class EntryFormDialog extends StatefulWidget {
  const EntryFormDialog({super.key, this.entry});
  final ItemEntry? entry;

  @override
  State<EntryFormDialog> createState() => _EntryFormDialogState();
}

class _EntryFormDialogState extends State<EntryFormDialog> {
  late final count = TextEditingController(text: '${widget.entry?.count ?? 1}');
  late final original = TextEditingController(
    text: widget.entry?.originalAmountPerUnit?.toString() ?? '',
  );
  late final current = TextEditingController(
    text: widget.entry?.currentAmountPerUnit?.toString() ?? '',
  );
  late var hasDetails = _hasDetails(widget.entry);
  late ItemUnit unit = widget.entry?.unit ?? ItemUnit.unspecified;
  late ItemUnit? lastConvertibleUnit = unit == ItemUnit.unspecified
      ? null
      : unit;
  late ConsumableState state =
      widget.entry?.state ?? ConsumableState.unspecified;
  DateTime? acquisitionDate;
  DateTime? expirationDate;
  final key = GlobalKey<FormState>();

  @override
  void initState() {
    super.initState();
    acquisitionDate = widget.entry?.acquisitionDate;
    expirationDate = widget.entry?.expirationDate;
  }

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
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: const Text('Add details'),
            subtitle: const Text('Quantity, state, and dates are optional'),
            value: hasDetails,
            onChanged: (value) => setState(() => hasDetails = value),
          ),
          if (hasDetails) ...[
            TextFormField(
              controller: original,
              keyboardType: const TextInputType.numberWithOptions(
                decimal: true,
              ),
              decoration: const InputDecoration(
                labelText: 'Original amount per unit',
              ),
              validator: _optionalQuantity,
              onChanged: (value) {
                if (state == ConsumableState.unopened) current.text = value;
              },
            ),
            if (state == ConsumableState.opened)
              TextFormField(
                controller: current,
                keyboardType: const TextInputType.numberWithOptions(
                  decimal: true,
                ),
                decoration: const InputDecoration(
                  labelText: 'Current amount per unit',
                ),
                validator: _optionalQuantity,
              ),
            DropdownButtonFormField<ItemUnit>(
              initialValue: unit,
              decoration: const InputDecoration(labelText: 'Unit'),
              items: ItemUnit.values
                  .map(
                    (unit) =>
                        DropdownMenuItem(value: unit, child: Text(unit.name)),
                  )
                  .toList(),
              onChanged: (value) => _changeUnit(value!),
            ),
            DropdownButtonFormField<ConsumableState>(
              initialValue: state,
              decoration: const InputDecoration(labelText: 'State'),
              items: ConsumableState.values
                  .map(
                    (state) =>
                        DropdownMenuItem(value: state, child: Text(state.name)),
                  )
                  .toList(),
              onChanged: (value) => setState(() {
                state = value!;
                if (state == ConsumableState.unopened) {
                  current.text = original.text;
                }
              }),
            ),
            _DateField(
              title: 'Acquisition date',
              value: acquisitionDate,
              onTap: () => _pickDate(acquisition: true),
            ),
            _DateField(
              title: 'Expiration date',
              value: expirationDate,
              onTap: () => _pickDate(acquisition: false),
            ),
          ],
        ],
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: _save,
        child: Text(widget.entry == null ? 'Add' : 'Save'),
      ),
    ],
  );

  void _save() {
    if (!key.currentState!.validate()) return;
    final originalAmount = hasDetails ? num.tryParse(original.text) : null;
    final currentAmount = state == ConsumableState.unopened
        ? originalAmount
        : (hasDetails ? num.tryParse(current.text) : null);
    if (originalAmount != null &&
        currentAmount != null &&
        currentAmount > originalAmount) {
      return;
    }
    Navigator.pop(
      context,
      ItemEntry(
        id: widget.entry?.id,
        count: int.parse(count.text),
        originalAmountPerUnit: originalAmount,
        currentAmountPerUnit: currentAmount,
        unit: hasDetails ? unit : ItemUnit.unspecified,
        state: hasDetails ? state : ConsumableState.unspecified,
        acquisitionDate: hasDetails ? acquisitionDate : null,
        expirationDate: hasDetails ? expirationDate : null,
      ),
    );
  }

  Future<void> _pickDate({required bool acquisition}) async {
    final initial = acquisition ? acquisitionDate : expirationDate;
    final selected = await showDatePicker(
      context: context,
      initialDate: initial ?? DateTime.now(),
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
      final fromUnit = lastConvertibleUnit;
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
      lastConvertibleUnit = nextUnit;
    });
  }

  static bool _hasDetails(ItemEntry? entry) =>
      entry != null &&
      (entry.originalAmountPerUnit != null ||
          entry.currentAmountPerUnit != null ||
          entry.unit != ItemUnit.unspecified ||
          entry.state != ConsumableState.unspecified ||
          entry.acquisitionDate != null ||
          entry.expirationDate != null);
}

class _DateField extends StatelessWidget {
  const _DateField({
    required this.title,
    required this.value,
    required this.onTap,
  });
  final String title;
  final DateTime? value;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => ListTile(
    contentPadding: EdgeInsets.zero,
    title: Text(title),
    subtitle: Text(value?.toIso8601String().split('T').first ?? 'Not set'),
    trailing: const Icon(Icons.calendar_today_outlined),
    onTap: onTap,
  );
}

String? _required(String? value) =>
    value?.trim().isEmpty ?? true ? 'Required' : null;
String? _optionalQuantity(String? value) {
  if (value == null || value.trim().isEmpty) return null;
  final parsed = num.tryParse(value);
  return parsed == null || parsed < 0 ? 'Enter a non-negative number' : null;
}

String? _count(String? value) {
  final valueAsInt = int.tryParse(value ?? '');
  return valueAsInt == null || valueAsInt < 1
      ? 'Enter a whole number of at least 1'
      : null;
}

String? _optional(String value) => value.trim().isEmpty ? null : value.trim();
