import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

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
import '../view_models/space_detail_view_model.dart';

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
  late final SpaceDetailViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = SpaceDetailViewModel(
      householdId: widget.householdId,
      householdName: widget.householdName,
      spaceId: widget.spaceId,
      spacesRepository: widget.spacesRepository,
      itemsRepository: widget.itemsRepository,
      session: widget.session,
    );
  }

  @override
  void didUpdateWidget(SpaceDetailScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
      householdId: widget.householdId,
      householdName: widget.householdName,
      spaceId: widget.spaceId,
      spacesRepository: widget.spacesRepository,
      itemsRepository: widget.itemsRepository,
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
    return ChangeNotifierProvider<SpaceDetailViewModel>.value(
      value: _viewModel,
      child: Consumer<SpaceDetailViewModel>(
        builder: (BuildContext context, SpaceDetailViewModel viewModel, _) {
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
                          EntityAvatar(id: viewModel.spaceId, name: 'Space'),
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
                          householdId: viewModel.householdId,
                          householdName: viewModel.householdName,
                          spaceId: viewModel.spaceId,
                          repository: viewModel.itemsRepository,
                          session: viewModel.session,
                        ),
                        _SubSpacesView(viewModel: viewModel),
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
        },
      ),
    );
  }

  void _addSubSpace() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Sub-space creation is ready for backend wiring.'),
      ),
    );
  }
}

class _SubSpacesView extends StatelessWidget {
  const _SubSpacesView({required this.viewModel});

  final SpaceDetailViewModel viewModel;

  @override
  Widget build(BuildContext context) {
    if (viewModel.isLoadingChildren) {
      return const LoadingView(label: 'Loading spaces...');
    }
    if (viewModel.childrenError != null) {
      return ErrorView(
        title: 'Could not load sub-spaces',
        error: viewModel.childrenError,
        stackTrace: viewModel.childrenStackTrace,
        onRetry: viewModel.loadChildren,
      );
    }

    final List<Space> spaces = viewModel.children;
    if (spaces.isEmpty) {
      return EmptyView(
        title: 'No sub-spaces yet',
        message: 'Add a nested space to keep this location organized.',
        action: FilledButton.icon(
          onPressed: () {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text(
                  'Sub-space creation is ready for backend wiring.',
                ),
              ),
            );
          },
          icon: const Icon(Icons.add),
          label: const Text('Add sub-space'),
        ),
      );
    }
    return ListView.separated(
      padding: const EdgeInsets.all(AppSpacing.lg),
      itemCount: spaces.length,
      separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.md),
      itemBuilder: (BuildContext context, int index) {
        final Space space = spaces[index];
        return AppCard(
          onTap: () => context.go(
            '/households/${viewModel.householdId}/spaces/${space.id}?name=${Uri.encodeQueryComponent(viewModel.householdName)}',
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
  }
}
