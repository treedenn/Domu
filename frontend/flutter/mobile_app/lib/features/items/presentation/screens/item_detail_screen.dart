import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/items_repository.dart';
import '../../domain/item.dart';
import '../../domain/item_entry.dart';
import '../view_models/item_detail_view_model.dart';

class ItemDetailScreen extends StatefulWidget {
  const ItemDetailScreen({
    required this.householdId,
    required this.spaceId,
    required this.itemId,
    required this.repository,
    this.session,
    super.key,
  });

  final String householdId;
  final String spaceId;
  final String itemId;
  final ItemsRepository repository;
  final AuthSession? session;

  @override
  State<ItemDetailScreen> createState() => _ItemDetailScreenState();
}

class _ItemDetailScreenState extends State<ItemDetailScreen> {
  late final ItemDetailViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = ItemDetailViewModel(
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void didUpdateWidget(ItemDetailScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void dispose() {
    _viewModel.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider<ItemDetailViewModel>.value(
      value: _viewModel,
      child: Consumer<ItemDetailViewModel>(
        builder: (BuildContext context, ItemDetailViewModel viewModel, _) {
          return Scaffold(
            body: _buildBody(context, viewModel),
            floatingActionButton: FloatingActionButton.extended(
              onPressed: _addEntry,
              icon: const Icon(Icons.add),
              label: const Text('Add entry'),
            ),
          );
        },
      ),
    );
  }

  Widget _buildBody(BuildContext context, ItemDetailViewModel viewModel) {
    if (viewModel.isLoading) {
      return const LoadingView(label: 'Loading item...');
    }
    if (viewModel.error != null) {
      return ErrorView(
        title: 'Could not load item',
        error: viewModel.error,
        stackTrace: viewModel.stackTrace,
        onRetry: viewModel.load,
      );
    }
    final Item? item = viewModel.item;
    if (item == null) {
      return const EmptyView(title: 'Item not found');
    }

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      children: <Widget>[
        AppCard(
          tonal: true,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(item.name, style: Theme.of(context).textTheme.titleLarge),
              if (item.barcode != null) ...<Widget>[
                const SizedBox(height: AppSpacing.sm),
                Row(
                  children: <Widget>[
                    Expanded(
                      child: Text(
                        item.barcode!,
                        style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          fontFeatures: const <FontFeature>[
                            FontFeature.tabularFigures(),
                          ],
                        ),
                      ),
                    ),
                    IconButton(
                      tooltip: 'Copy barcode',
                      onPressed: () {
                        Clipboard.setData(ClipboardData(text: item.barcode!));
                      },
                      icon: const Icon(Icons.copy),
                    ),
                  ],
                ),
              ],
              const SizedBox(height: AppSpacing.md),
              Text(
                'x ${item.totalQuantity} total - ${item.entryCount} entries',
              ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.lg),
        Text('Entries', style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: AppSpacing.md),
        if (viewModel.entries.isEmpty)
          EmptyView(
            title: 'No entries yet',
            action: FilledButton(
              onPressed: _addEntry,
              child: const Text('Add entry'),
            ),
          )
        else
          for (final ItemEntry entry in viewModel.entries) ...<Widget>[
            AppCard(
              onTap: () => context.go(
                '/households/${viewModel.householdId}/spaces/${viewModel.spaceId}/items/${viewModel.itemId}/entries/${entry.id}',
              ),
              child: Row(
                children: <Widget>[
                  StateChip(state: entry.state, dense: true),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(child: Text('x ${entry.quantity}')),
                  ExpirationBadge(expiresAt: entry.expiresAt, verbose: true),
                ],
              ),
            ),
            const SizedBox(height: AppSpacing.md),
          ],
      ],
    );
  }

  void _addEntry() {
    context.go(
      '/households/${widget.householdId}/spaces/${widget.spaceId}/items/${widget.itemId}/entries/new',
    );
  }
}
