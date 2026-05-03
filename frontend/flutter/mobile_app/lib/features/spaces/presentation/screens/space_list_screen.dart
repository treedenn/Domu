import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../features/items/data/items_repository.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/spaces_repository.dart';
import '../../domain/space.dart';
import '../view_models/space_list_view_model.dart';

class SpaceListScreen extends StatefulWidget {
  const SpaceListScreen({
    required this.householdId,
    required this.householdName,
    required this.itemsRepository,
    this.repository,
    this.session,
    super.key,
  });

  final String householdId;
  final String householdName;
  final ItemsRepository itemsRepository;
  final SpacesRepository? repository;
  final AuthSession? session;

  @override
  State<SpaceListScreen> createState() => _SpaceListScreenState();
}

class _SpaceListScreenState extends State<SpaceListScreen> {
  final TextEditingController _searchController = TextEditingController();
  late final SpaceListViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = SpaceListViewModel(
      householdId: widget.householdId,
      householdName: widget.householdName,
      itemsRepository: widget.itemsRepository,
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void didUpdateWidget(SpaceListScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      householdName: widget.householdName,
      itemsRepository: widget.itemsRepository,
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
    return ChangeNotifierProvider<SpaceListViewModel>.value(
      value: _viewModel,
      child: Consumer<SpaceListViewModel>(
        builder: (BuildContext context, SpaceListViewModel viewModel, _) {
          return Scaffold(
            body: _buildBody(context, viewModel),
            floatingActionButton: FloatingActionButton.extended(
              onPressed: () => _showAddSpaceSheet(context, viewModel),
              icon: const Icon(Icons.add),
              label: const Text('Add space'),
            ),
          );
        },
      ),
    );
  }

  Widget _buildBody(BuildContext context, SpaceListViewModel viewModel) {
    if (viewModel.isLoading) {
      return const LoadingView(label: 'Loading spaces...');
    }
    if (viewModel.error != null) {
      return ErrorView(
        title: 'Could not load spaces',
        error: viewModel.error,
        stackTrace: viewModel.stackTrace,
        onRetry: viewModel.load,
      );
    }

    final List<Space> allSpaces = viewModel.allSpaces;
    final List<Space> spaces = viewModel.spaces;

    if (allSpaces.isEmpty) {
      return EmptyView(
        title: 'Create your first space',
        message:
            'Spaces can be rooms, cupboards, boxes, or any nested place you store things.',
        action: FilledButton.icon(
          onPressed: () => _showAddSpaceSheet(context, viewModel),
          icon: const Icon(Icons.add),
          label: const Text('Add space'),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: viewModel.load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.lg),
        children: <Widget>[
          AppSearchField(
            controller: _searchController,
            hintText: 'Search spaces',
            onChanged: viewModel.updateQuery,
          ),
          const SizedBox(height: AppSpacing.md),
          Callout(
            severity: CalloutSeverity.warning,
            message: 'Items expiring soon need attention',
            actionLabel: 'View',
            onAction: () => context.go(
              '/households/${viewModel.householdId}/search?name=${Uri.encodeQueryComponent(viewModel.householdName)}&expiring=7d',
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          for (final Space space in spaces) ...<Widget>[
            _SpaceCard(
              space: space,
              onTap: () => context.go(
                '/households/${viewModel.householdId}/spaces/${space.id}?name=${Uri.encodeQueryComponent(viewModel.householdName)}',
              ),
            ),
            const SizedBox(height: AppSpacing.md),
          ],
        ],
      ),
    );
  }

  void _showAddSpaceSheet(BuildContext context, SpaceListViewModel viewModel) {
    final TextEditingController nameController = TextEditingController();
    final TextEditingController descriptionController = TextEditingController();
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
              Text('Add space', style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: AppSpacing.lg),
              TextField(
                controller: nameController,
                decoration: const InputDecoration(labelText: 'Name'),
              ),
              const SizedBox(height: AppSpacing.md),
              TextField(
                controller: descriptionController,
                minLines: 2,
                maxLines: 4,
                decoration: const InputDecoration(labelText: 'Description'),
              ),
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: () async {
                  await viewModel.createSpace(
                    name: nameController.text.trim(),
                    description: descriptionController.text.trim(),
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
      descriptionController.dispose();
    });
  }
}

class _SpaceCard extends StatelessWidget {
  const _SpaceCard({required this.space, required this.onTap});

  final Space space;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      onTap: onTap,
      child: Row(
        children: <Widget>[
          EntityAvatar(id: space.id, name: space.name),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  space.name,
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: AppSpacing.xs),
                Text(
                  _subtitle(space),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: <Widget>[
              Text('${space.itemCount} items'),
              Text('${space.childSpaceCount} spaces'),
            ],
          ),
          const SizedBox(width: AppSpacing.sm),
          const Icon(Icons.chevron_right),
        ],
      ),
    );
  }

  String _subtitle(Space space) {
    final List<String> parts = <String>[
      if (space.description != null && space.description!.trim().isNotEmpty)
        space.description!.trim(),
      if (space.childSpaceCount > 0) '${space.childSpaceCount} sub-spaces',
    ];
    return parts.isEmpty ? 'No description' : parts.join(' - ');
  }
}
