import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item.dart';
import '../view_models/item_list_view_model.dart';

class ItemListView extends StatefulWidget {
  const ItemListView({
    required this.householdId,
    required this.householdName,
    required this.spaceId,
    required this.repository,
    this.session,
    super.key,
  });

  final String householdId;
  final String householdName;
  final String spaceId;
  final ItemsRepository repository;
  final AuthSession? session;

  @override
  State<ItemListView> createState() => _ItemListViewState();
}

class _ItemListViewState extends State<ItemListView> {
  final TextEditingController _searchController = TextEditingController();
  late final ItemListViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = ItemListViewModel(
      householdId: widget.householdId,
      householdName: widget.householdName,
      spaceId: widget.spaceId,
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void didUpdateWidget(ItemListView oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      householdName: widget.householdName,
      spaceId: widget.spaceId,
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void dispose() {
    _searchController.dispose();
    _viewModel.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider<ItemListViewModel>.value(
      value: _viewModel,
      child: Consumer<ItemListViewModel>(
        builder: (BuildContext context, ItemListViewModel viewModel, _) {
          if (viewModel.isLoading) {
            return const LoadingView(label: 'Loading items...');
          }
          if (viewModel.error != null) {
            return ErrorView(
              title: 'Could not load items',
              error: viewModel.error,
              stackTrace: viewModel.stackTrace,
              onRetry: viewModel.load,
            );
          }
          final List<Item> items = viewModel.items;
          if (viewModel.allItems.isEmpty) {
            return EmptyView(
              title: 'No items yet',
              message: 'Add the first item stored in this space.',
              action: FilledButton.icon(
                onPressed: () => _showAddItemSheet(viewModel),
                icon: const Icon(Icons.add),
                label: const Text('Add item'),
              ),
            );
          }
          return ListView(
            padding: const EdgeInsets.all(AppSpacing.lg),
            children: <Widget>[
              AppSearchField(
                controller: _searchController,
                hintText: 'Search items',
                onChanged: viewModel.updateQuery,
              ),
              const SizedBox(height: AppSpacing.md),
              FilterChipStrip<ConsumableState?>(
                defaultValue: null,
                selected: viewModel.stateFilter,
                onSelected: viewModel.updateStateFilter,
                options: const <FilterChipOption<ConsumableState?>>[
                  FilterChipOption<ConsumableState?>(
                    value: null,
                    label: 'Any state',
                  ),
                  FilterChipOption<ConsumableState?>(
                    value: ConsumableState.unopened,
                    label: 'Unopened',
                  ),
                  FilterChipOption<ConsumableState?>(
                    value: ConsumableState.opened,
                    label: 'Opened',
                  ),
                  FilterChipOption<ConsumableState?>(
                    value: ConsumableState.unknown,
                    label: 'Unknown',
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              for (final Item item in items) ...<Widget>[
                AppCard(
                  onTap: () => context.go(
                    '/households/${viewModel.householdId}/spaces/${viewModel.spaceId}/items/${item.id}?name=${Uri.encodeQueryComponent(viewModel.householdName)}',
                  ),
                  child: Row(
                    children: <Widget>[
                      Icon(
                        item.barcode == null
                            ? Icons.inventory_2_outlined
                            : Icons.qr_code_2,
                      ),
                      const SizedBox(width: AppSpacing.md),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: <Widget>[
                            Text(
                              item.name,
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                            const SizedBox(height: AppSpacing.sm),
                            Wrap(
                              spacing: AppSpacing.sm,
                              runSpacing: AppSpacing.xs,
                              children: <Widget>[
                                StateChip(
                                  state: item.dominantState,
                                  dense: true,
                                ),
                                ExpirationBadge(
                                  expiresAt: item.earliestExpiresAt,
                                ),
                                Chip(label: Text('x ${item.totalQuantity}')),
                              ],
                            ),
                          ],
                        ),
                      ),
                      const Icon(Icons.chevron_right),
                    ],
                  ),
                ),
                const SizedBox(height: AppSpacing.md),
              ],
            ],
          );
        },
      ),
    );
  }

  void _showAddItemSheet(ItemListViewModel viewModel) {
    final TextEditingController nameController = TextEditingController();
    final TextEditingController barcodeController = TextEditingController();
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (BuildContext context) {
        return Padding(
          padding: EdgeInsets.only(
            left: AppSpacing.lg,
            right: AppSpacing.lg,
            bottom: MediaQuery.viewInsetsOf(context).bottom + AppSpacing.lg,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              Text('Add item', style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: AppSpacing.lg),
              TextField(
                controller: nameController,
                decoration: const InputDecoration(labelText: 'Name'),
              ),
              const SizedBox(height: AppSpacing.md),
              TextField(
                controller: barcodeController,
                decoration: InputDecoration(
                  labelText: 'Barcode',
                  suffixIcon: IconButton(
                    tooltip: 'Scan barcode',
                    onPressed: () async {
                      final String? code = await showScannerSheet(context);
                      if (code != null) {
                        barcodeController.text = code;
                      }
                    },
                    icon: const Icon(Icons.qr_code_scanner),
                  ),
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: () async {
                  await viewModel.addItem(
                    name: nameController.text.trim(),
                    barcode: barcodeController.text.trim(),
                  );
                  if (context.mounted) {
                    Navigator.of(context).pop();
                  }
                },
                child: const Text('Save'),
              ),
            ],
          ),
        );
      },
    ).whenComplete(() {
      nameController.dispose();
      barcodeController.dispose();
    });
  }
}
