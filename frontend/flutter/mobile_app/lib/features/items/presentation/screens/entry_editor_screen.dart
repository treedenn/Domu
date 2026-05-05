import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item_container_type.dart';
import '../../domain/item_unit.dart';
import '../view_models/entry_editor_view_model.dart';

class EntryEditorScreen extends StatefulWidget {
  const EntryEditorScreen({
    required this.itemId,
    required this.repository,
    required this.householdId,
    required this.spaceId,
    this.session,
    this.entryId,
    super.key,
  });

  final String householdId;
  final String spaceId;
  final String itemId;
  final String? entryId;
  final ItemsRepository repository;
  final AuthSession? session;

  @override
  State<EntryEditorScreen> createState() => _EntryEditorScreenState();
}

class _EntryEditorScreenState extends State<EntryEditorScreen> {
  late final EntryEditorViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = EntryEditorViewModel(
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
      repository: widget.repository,
      session: widget.session,
      entryId: widget.entryId,
    );
  }

  @override
  void didUpdateWidget(EntryEditorScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
      repository: widget.repository,
      session: widget.session,
      entryId: widget.entryId,
    );
  }

  @override
  void dispose() {
    _viewModel.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider<EntryEditorViewModel>.value(
      value: _viewModel,
      child: Consumer<EntryEditorViewModel>(
        builder: (BuildContext context, EntryEditorViewModel viewModel, _) {
          return Scaffold(
            body: ListView(
              padding: const EdgeInsets.all(AppSpacing.lg),
              children: <Widget>[
                AppCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Text(
                        'Amount',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        initialValue: _formatNumber(viewModel.initialQuantity),
                        keyboardType: const TextInputType.numberWithOptions(
                          decimal: true,
                        ),
                        decoration: const InputDecoration(
                          labelText: 'Initial quantity',
                        ),
                        onChanged: (String value) {
                          final double? quantity = double.tryParse(value);
                          if (quantity != null) {
                            viewModel.updateInitialQuantity(quantity);
                          }
                        },
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        initialValue: _formatNumber(viewModel.currentQuantity),
                        keyboardType: const TextInputType.numberWithOptions(
                          decimal: true,
                        ),
                        decoration: const InputDecoration(
                          labelText: 'Current quantity',
                        ),
                        onChanged: (String value) {
                          final double? quantity = double.tryParse(value);
                          if (quantity != null) {
                            viewModel.updateCurrentQuantity(quantity);
                          }
                        },
                      ),
                      const SizedBox(height: AppSpacing.md),
                      DropdownButtonFormField<ItemUnit>(
                        initialValue: viewModel.unit,
                        decoration: const InputDecoration(labelText: 'Unit'),
                        items: ItemUnit.values
                            .where(
                              (ItemUnit unit) => unit != ItemUnit.unspecified,
                            )
                            .map(
                              (ItemUnit unit) => DropdownMenuItem<ItemUnit>(
                                value: unit,
                                child: Text(
                                  '${unit.label} (${unit.kind.label.toLowerCase()})',
                                ),
                              ),
                            )
                            .toList(growable: false),
                        onChanged: (ItemUnit? value) {
                          if (value != null) {
                            viewModel.updateUnit(value);
                          }
                        },
                      ),
                      const SizedBox(height: AppSpacing.md),
                      DropdownButtonFormField<ItemContainerType>(
                        initialValue: viewModel.containerType,
                        decoration: const InputDecoration(
                          labelText: 'Container',
                        ),
                        items: ItemContainerType.values
                            .map(
                              (ItemContainerType containerType) =>
                                  DropdownMenuItem<ItemContainerType>(
                                    value: containerType,
                                    child: Text(containerType.label),
                                  ),
                            )
                            .toList(growable: false),
                        onChanged: (ItemContainerType? value) {
                          if (value != null) {
                            viewModel.updateContainerType(value);
                          }
                        },
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: AppSpacing.md),
                AppCard(
                  child: Column(
                    children: <Widget>[
                      DateField(
                        labelText: 'Acquired on',
                        value: viewModel.acquiredAt,
                        onChanged: (DateTime? value) {
                          if (value != null) {
                            viewModel.updateAcquiredAt(value);
                          }
                        },
                        firstDate: DateTime(2000),
                        lastDate: DateTime.now(),
                      ),
                      const SizedBox(height: AppSpacing.md),
                      DateField(
                        labelText: 'Expires on',
                        value: viewModel.expiresAt,
                        clearable: true,
                        onChanged: viewModel.updateExpiresAt,
                        firstDate: DateTime(2000),
                        lastDate: DateTime(2100),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: AppSpacing.md),
                AppCard(
                  child: SegmentedButton<ConsumableState>(
                    segments: const <ButtonSegment<ConsumableState>>[
                      ButtonSegment<ConsumableState>(
                        value: ConsumableState.unknown,
                        label: Text('Unknown'),
                      ),
                      ButtonSegment<ConsumableState>(
                        value: ConsumableState.unopened,
                        label: Text('Unopened'),
                      ),
                      ButtonSegment<ConsumableState>(
                        value: ConsumableState.opened,
                        label: Text('Opened'),
                      ),
                    ],
                    selected: <ConsumableState>{viewModel.state},
                    onSelectionChanged: (Set<ConsumableState> value) {
                      viewModel.updateState(value.first);
                    },
                  ),
                ),
                if (viewModel.error != null) ...<Widget>[
                  const SizedBox(height: AppSpacing.md),
                  Text(
                    viewModel.error!,
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.error,
                    ),
                  ),
                ],
              ],
            ),
            bottomNavigationBar: SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: Row(
                  children: <Widget>[
                    Expanded(
                      child: OutlinedButton(
                        onPressed: viewModel.isSaving
                            ? null
                            : () => Navigator.of(context).pop(),
                        child: const Text('Cancel'),
                      ),
                    ),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(
                      child: FilledButton(
                        onPressed: viewModel.isSaving
                            ? null
                            : () => _save(context, viewModel),
                        child: const Text('Save'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Future<void> _save(
    BuildContext context,
    EntryEditorViewModel viewModel,
  ) async {
    final bool saved = await viewModel.save();
    if (saved && context.mounted) {
      Navigator.of(context).pop();
    }
  }

  String _formatNumber(double value) {
    if (value == value.roundToDouble()) {
      return value.toStringAsFixed(0);
    }
    return value.toString();
  }
}
