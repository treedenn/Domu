import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../../items/domain/item.dart';
import '../../../spaces/domain/space.dart';
import '../../data/search_repository.dart';
import '../../domain/search_engine.dart';
import '../../domain/search_query.dart';

class SearchScreen extends StatefulWidget {
  const SearchScreen({
    required this.householdId,
    required this.repository,
    this.session,
    this.initialExpiringFilter,
    super.key,
  });

  final String householdId;
  final SearchRepository repository;
  final AuthSession? session;
  final String? initialExpiringFilter;

  @override
  State<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends State<SearchScreen> {
  Future<SearchResults>? _results;
  final TextEditingController _controller = TextEditingController();
  Timer? _debounce;
  String _query = '';

  int? get _expiringDays => widget.initialExpiringFilter == '7d' ? 7 : null;

  @override
  void initState() {
    super.initState();
    if (_expiringDays != null) {
      _results = _search();
    }
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _controller.dispose();
    super.dispose();
  }

  Future<SearchResults> _search() async {
    final AuthSession? session = widget.session;
    if (session == null) {
      return const SearchResults(spaces: <Space>[], items: <Item>[]);
    }
    return widget.repository.search(
      session: session,
      householdId: widget.householdId,
      query: SearchQuery(text: _query, expiringWithinDays: _expiringDays),
    );
  }

  @override
  Widget build(BuildContext context) {
    final bool emptyQuery = _query.trim().isEmpty && _expiringDays == null;

    return FutureBuilder<SearchResults>(
      future: _results,
      builder: (BuildContext context, AsyncSnapshot<SearchResults> snapshot) {
        if (emptyQuery) {
          return _SearchScaffold(
            controller: _controller,
            onChanged: _onQueryChanged,
            child: const EmptyView(
              title: 'Search items, spaces, barcodes',
              message: 'Results appear as you type.',
            ),
          );
        }
        if (snapshot.connectionState != ConnectionState.done) {
          return const LoadingView(label: 'Loading search...');
        }
        if (snapshot.hasError) {
          return ErrorView(
            title: 'Could not load search',
            message: snapshot.error.toString(),
            onRetry: () => setState(() => _results = _search()),
          );
        }
        final SearchResults results =
            snapshot.data ??
            const SearchResults(spaces: <Space>[], items: <Item>[]);

        return _SearchScaffold(
          controller: _controller,
          onChanged: _onQueryChanged,
          child: results.items.isEmpty && results.spaces.isEmpty
              ? const EmptyView(title: 'No matches in this household')
              : Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    if (results.spaces.isNotEmpty) ...<Widget>[
                      Text(
                        'Spaces',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      for (final Space space in results.spaces) ...<Widget>[
                        AppCard(
                          onTap: () => context.go(
                            '/households/${widget.householdId}/spaces/${space.id}',
                          ),
                          child: Row(
                            children: <Widget>[
                              EntityAvatar(id: space.id, name: space.name),
                              const SizedBox(width: AppSpacing.md),
                              Expanded(child: Text(space.name)),
                              const Icon(Icons.chevron_right),
                            ],
                          ),
                        ),
                        const SizedBox(height: AppSpacing.md),
                      ],
                    ],
                    if (results.items.isNotEmpty) ...<Widget>[
                      Text(
                        'Items',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      for (final Item item in results.items) ...<Widget>[
                        AppCard(
                          onTap: () => context.go(
                            '/households/${widget.householdId}/spaces/${item.spaceId}/items/${item.id}',
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
                                    Text(item.name),
                                    Text(
                                      item.barcode ?? 'No barcode',
                                      style: Theme.of(
                                        context,
                                      ).textTheme.bodySmall,
                                    ),
                                  ],
                                ),
                              ),
                              ExpirationBadge(
                                expiresAt: item.earliestExpiresAt,
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: AppSpacing.md),
                      ],
                    ],
                  ],
                ),
        );
      },
    );
  }

  void _onQueryChanged(String value) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 200), () {
      if (!mounted) {
        return;
      }

      setState(() {
        _query = value;
        _results = _query.trim().isEmpty && _expiringDays == null
            ? null
            : _search();
      });
    });
  }
}

class _SearchScaffold extends StatelessWidget {
  const _SearchScaffold({
    required this.controller,
    required this.onChanged,
    required this.child,
  });

  final TextEditingController controller;
  final ValueChanged<String> onChanged;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      children: <Widget>[
        AppSearchField(
          controller: controller,
          autofocus: true,
          hintText: 'Search items, spaces, barcodes',
          onChanged: onChanged,
        ),
        const SizedBox(height: AppSpacing.lg),
        child,
      ],
    );
  }
}
