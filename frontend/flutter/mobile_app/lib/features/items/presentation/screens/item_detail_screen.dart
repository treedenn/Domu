import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/items_repository.dart';
import '../../domain/item.dart';
import '../../domain/item_entry.dart';

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
  late Future<({Item? item, List<ItemEntry> entries})> _details;

  @override
  void initState() {
    super.initState();
    _details = _load();
  }

  Future<({Item? item, List<ItemEntry> entries})> _load() async {
    final AuthSession? session = widget.session;
    if (session == null) {
      return (item: null, entries: const <ItemEntry>[]);
    }
    final Item? item = await widget.repository.getItem(
      session: session,
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
    );
    final List<ItemEntry> entries = await widget.repository.getEntries(
      session: session,
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      itemId: widget.itemId,
    );
    return (item: item, entries: entries);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: FutureBuilder<({Item? item, List<ItemEntry> entries})>(
        future: _details,
        builder: (BuildContext context, AsyncSnapshot<({Item? item, List<ItemEntry> entries})> snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView(label: 'Loading item...');
          }
          if (snapshot.hasError) {
            return ErrorView(
              title: 'Could not load item',
              message: snapshot.error.toString(),
              onRetry: _reload,
            );
          }
          final Item? item = snapshot.data?.item;
          if (item == null) {
            return const EmptyView(title: 'Item not found');
          }
          final List<ItemEntry> entries = snapshot.data?.entries ?? const <ItemEntry>[];
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
                                    fontFeatures: const <FontFeature>[FontFeature.tabularFigures()],
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
                    Text('x ${item.totalQuantity} total - ${item.entryCount} entries'),
                  ],
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              Text('Entries', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: AppSpacing.md),
              if (entries.isEmpty)
                EmptyView(
                  title: 'No entries yet',
                  action: FilledButton(
                    onPressed: _addEntry,
                    child: const Text('Add entry'),
                  ),
                )
              else
                for (final ItemEntry entry in entries) ...<Widget>[
                  AppCard(
                    onTap: () => context.go(
                      '/households/${widget.householdId}/spaces/${widget.spaceId}/items/${widget.itemId}/entries/${entry.id}',
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
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _addEntry,
        icon: const Icon(Icons.add),
        label: const Text('Add entry'),
      ),
    );
  }

  void _addEntry() {
    context.go(
      '/households/${widget.householdId}/spaces/${widget.spaceId}/items/${widget.itemId}/entries/new',
    );
  }

  void _reload() {
    setState(() {
      _details = _load();
    });
  }
}
