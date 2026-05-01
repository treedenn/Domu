import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../features/items/data/items_repository.dart';
import '../../../../features/items/presentation/screens/item_list_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/spaces_repository.dart';
import '../../domain/space.dart';

class SpaceDetailScreen extends StatefulWidget {
  const SpaceDetailScreen({
    required this.householdId,
    required this.householdName,
    required this.spaceId,
    required this.spacesRepository,
    required this.itemsRepository,
    this.session,
    super.key,
  });

  final String householdId;
  final String householdName;
  final String spaceId;
  final SpacesRepository spacesRepository;
  final ItemsRepository itemsRepository;
  final AuthSession? session;

  @override
  State<SpaceDetailScreen> createState() => _SpaceDetailScreenState();
}

class _SpaceDetailScreenState extends State<SpaceDetailScreen> {
  late Future<SpacePage> _children;

  @override
  void initState() {
    super.initState();
    _children = _loadChildren();
  }

  Future<SpacePage> _loadChildren() {
    final AuthSession? session = widget.session;
    if (session == null) {
      return Future<SpacePage>.value(
        const SpacePage(
          spaces: <Space>[],
          pageNumber: 1,
          pageSize: 20,
          totalCount: 0,
        ),
      );
    }
    return widget.spacesRepository.getSpaces(
      session: session,
      householdId: widget.householdId,
      parentId: widget.spaceId,
    );
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        body: Column(
          children: <Widget>[
            Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: AppCard(
                tonal: true,
                child: Row(
                  children: <Widget>[
                    EntityAvatar(id: widget.spaceId, name: 'Space'),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(
                      child: Text(
                        'Space details',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const TabBar(
              tabs: <Widget>[
                Tab(text: 'Items'),
                Tab(text: 'Sub-spaces'),
              ],
            ),
            Expanded(
              child: TabBarView(
                children: <Widget>[
                  ItemListView(
                    householdId: widget.householdId,
                    householdName: widget.householdName,
                    spaceId: widget.spaceId,
                    repository: widget.itemsRepository,
                    session: widget.session,
                  ),
                  FutureBuilder<SpacePage>(
                    future: _children,
                    builder: (
                      BuildContext context,
                      AsyncSnapshot<SpacePage> snapshot,
                    ) {
                      if (snapshot.connectionState != ConnectionState.done) {
                        return const LoadingView(label: 'Loading spaces...');
                      }
                      if (snapshot.hasError) {
                        return ErrorView(
                          title: 'Could not load sub-spaces',
                          message: snapshot.error.toString(),
                          onRetry: () {
                            setState(() => _children = _loadChildren());
                          },
                        );
                      }
                      final List<Space> spaces =
                          snapshot.data?.spaces ?? const <Space>[];
                      if (spaces.isEmpty) {
                        return EmptyView(
                          title: 'No sub-spaces yet',
                          message: 'Add a nested space to keep this location organized.',
                          action: FilledButton.icon(
                            onPressed: _addSubSpace,
                            icon: const Icon(Icons.add),
                            label: const Text('Add sub-space'),
                          ),
                        );
                      }
                      return ListView.separated(
                        padding: const EdgeInsets.all(AppSpacing.lg),
                        itemCount: spaces.length,
                        separatorBuilder: (_, _) =>
                            const SizedBox(height: AppSpacing.md),
                        itemBuilder: (BuildContext context, int index) {
                          final Space space = spaces[index];
                          return AppCard(
                            onTap: () => context.go(
                              '/households/${widget.householdId}/spaces/${space.id}?name=${Uri.encodeQueryComponent(widget.householdName)}',
                            ),
                            child: Row(
                              children: <Widget>[
                                EntityAvatar(id: space.id, name: space.name),
                                const SizedBox(width: AppSpacing.md),
                                Expanded(child: Text(space.name)),
                                const Icon(Icons.chevron_right),
                              ],
                            ),
                          );
                        },
                      );
                    },
                  ),
                ],
              ),
            ),
          ],
        ),
        floatingActionButton: FloatingActionButton.extended(
          onPressed: _addSubSpace,
          icon: const Icon(Icons.add),
          label: const Text('Add'),
        ),
      ),
    );
  }

  void _addSubSpace() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Sub-space creation is ready for backend wiring.')),
    );
  }
}
