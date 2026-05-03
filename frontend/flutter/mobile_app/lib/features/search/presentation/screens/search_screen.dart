import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

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
import '../view_models/search_view_model.dart';

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
  final TextEditingController _controller = TextEditingController();
  late final SearchViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = SearchViewModel(
      householdId: widget.householdId,
      repository: widget.repository,
      session: widget.session,
      initialExpiringFilter: widget.initialExpiringFilter,
    );
  }

  @override
  void didUpdateWidget(SearchScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      repository: widget.repository,
      session: widget.session,
      initialExpiringFilter: widget.initialExpiringFilter,
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    _viewModel.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider<SearchViewModel>.value(
      value: _viewModel,
      child: Consumer<SearchViewModel>(
        builder: (BuildContext context, SearchViewModel viewModel, _) {
          if (viewModel.emptyQuery) {
            return _SearchScaffold(
              controller: _controller,
              onChanged: viewModel.updateQuery,
              child: const EmptyView(
                title: 'Search items, spaces, barcodes',
                message: 'Results appear as you type.',
              ),
            );
          }
          if (viewModel.isLoading) {
            return const LoadingView(label: 'Loading search...');
          }
          if (viewModel.error != null) {
            return ErrorView(
              title: 'Could not load search',
              error: viewModel.error,
              stackTrace: viewModel.stackTrace,
              onRetry: viewModel.retry,
            );
          }
          final SearchResults results =
              viewModel.results ??
              const SearchResults(spaces: <Space>[], items: <Item>[]);

          return _SearchScaffold(
            controller: _controller,
            onChanged: viewModel.updateQuery,
            child: results.items.isEmpty && results.spaces.isEmpty
                ? const EmptyView(title: 'No matches in this household')
                : _SearchResultsView(
                    householdId: widget.householdId,
                    results: results,
                  ),
          );
        },
      ),
    );
  }
}

class _SearchResultsView extends StatelessWidget {
  const _SearchResultsView({required this.householdId, required this.results});

  final String householdId;
  final SearchResults results;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        if (results.spaces.isNotEmpty) ...<Widget>[
          Text('Spaces', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: AppSpacing.md),
          for (final Space space in results.spaces) ...<Widget>[
            AppCard(
              onTap: () =>
                  context.go('/households/$householdId/spaces/${space.id}'),
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
          Text('Items', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: AppSpacing.md),
          for (final Item item in results.items) ...<Widget>[
            AppCard(
              onTap: () => context.go(
                '/households/$householdId/spaces/${item.spaceId}/items/${item.id}',
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
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ],
                    ),
                  ),
                  ExpirationBadge(expiresAt: item.earliestExpiresAt),
                ],
              ),
            ),
            const SizedBox(height: AppSpacing.md),
          ],
        ],
      ],
    );
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
