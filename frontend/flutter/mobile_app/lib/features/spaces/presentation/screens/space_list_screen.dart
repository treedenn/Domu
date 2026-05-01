import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../features/items/data/items_repository.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/spaces_repository.dart';
import '../../domain/space.dart';

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
  late Future<SpacePage> _spaces;
  final TextEditingController _searchController = TextEditingController();
  String _query = '';

  @override
  void initState() {
    super.initState();
    _spaces = _loadSpaces();
  }

  @override
  void didUpdateWidget(SpaceListScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.repository != widget.repository ||
        oldWidget.session != widget.session ||
        oldWidget.householdId != widget.householdId) {
      _spaces = _loadSpaces();
    }
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<SpacePage> _loadSpaces() {
    final SpacesRepository? repository = widget.repository;
    final AuthSession? session = widget.session;
    if (repository == null || session == null) {
      return Future<SpacePage>.value(
        const SpacePage(
          spaces: <Space>[],
          pageNumber: 1,
          pageSize: 20,
          totalCount: 0,
        ),
      );
    }

    return repository.getSpaces(
      session: session,
      householdId: widget.householdId,
    );
  }

  void _reload() {
    setState(() {
      _spaces = _loadSpaces();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: FutureBuilder<SpacePage>(
        future: _spaces,
        builder: (BuildContext context, AsyncSnapshot<SpacePage> snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView(label: 'Loading spaces...');
          }
          if (snapshot.hasError) {
            return ErrorView(
              title: 'Could not load spaces',
              message: snapshot.error.toString(),
              onRetry: _reload,
            );
          }

          final List<Space> allSpaces =
              snapshot.data?.spaces ?? const <Space>[];
          final List<Space> spaces = allSpaces
              .where((Space space) =>
                  space.name.toLowerCase().contains(_query.toLowerCase()))
              .toList(growable: false);

          if (allSpaces.isEmpty) {
            return EmptyView(
              title: 'Create your first space',
              message: 'Spaces can be rooms, cupboards, boxes, or any nested place you store things.',
              action: FilledButton.icon(
                onPressed: () => _showAddSpaceSheet(context),
                icon: const Icon(Icons.add),
                label: const Text('Add space'),
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: () async => _reload(),
            child: ListView(
              padding: const EdgeInsets.all(AppSpacing.lg),
              children: <Widget>[
                AppSearchField(
                  controller: _searchController,
                  hintText: 'Search spaces',
                  onChanged: (String value) {
                    setState(() => _query = value);
                  },
                ),
                const SizedBox(height: AppSpacing.md),
                Callout(
                  severity: CalloutSeverity.warning,
                  message: 'Items expiring soon need attention',
                  actionLabel: 'View',
                  onAction: () => context.go(
                    '/households/${widget.householdId}/search?name=${Uri.encodeQueryComponent(widget.householdName)}&expiring=7d',
                  ),
                ),
                const SizedBox(height: AppSpacing.md),
                for (final Space space in spaces) ...<Widget>[
                  _SpaceCard(
                    space: space,
                    onTap: () => context.go(
                      '/households/${widget.householdId}/spaces/${space.id}?name=${Uri.encodeQueryComponent(widget.householdName)}',
                    ),
                  ),
                  const SizedBox(height: AppSpacing.md),
                ],
              ],
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showAddSpaceSheet(context),
        icon: const Icon(Icons.add),
        label: const Text('Add space'),
      ),
    );
  }

  void _showAddSpaceSheet(BuildContext context) {
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
                  final AuthSession? session = widget.session;
                  final SpacesRepository? repository = widget.repository;
                  if (session != null && repository != null) {
                    await repository.create(
                      session: session,
                      householdId: widget.householdId,
                      name: nameController.text.trim(),
                      description: descriptionController.text.trim(),
                    );
                    _reload();
                  }
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
                Text(space.name, style: Theme.of(context).textTheme.titleMedium),
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
