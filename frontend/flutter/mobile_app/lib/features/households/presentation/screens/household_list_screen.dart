import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/households_repository.dart';
import '../../domain/household.dart';
import '../view_models/household_list_view_model.dart';

class HouseholdListScreen extends StatefulWidget {
  const HouseholdListScreen({
    this.repository,
    this.session,
    this.onSignOut,
    super.key,
  });

  final HouseholdsRepository? repository;
  final AuthSession? session;
  final Future<void> Function()? onSignOut;

  @override
  State<HouseholdListScreen> createState() => _HouseholdListScreenState();
}

class _HouseholdListScreenState extends State<HouseholdListScreen> {
  late final HouseholdListViewModel _viewModel;

  @override
  void initState() {
    super.initState();
    _viewModel = HouseholdListViewModel(
      repository: widget.repository,
      session: widget.session,
    );
  }

  @override
  void didUpdateWidget(HouseholdListScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _viewModel.updateDependencies(
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
    return ChangeNotifierProvider<HouseholdListViewModel>.value(
      value: _viewModel,
      child: Consumer<HouseholdListViewModel>(
        builder: (BuildContext context, HouseholdListViewModel viewModel, _) {
          return Scaffold(
            appBar: AppBar(
              title: const Text('Households'),
              actions: <Widget>[
                if (widget.onSignOut != null)
                  IconButton(
                    onPressed: widget.onSignOut,
                    tooltip: 'Sign out',
                    icon: const Icon(Icons.logout),
                  ),
              ],
            ),
            body: _buildBody(context, viewModel),
            floatingActionButton: FloatingActionButton.extended(
              onPressed: () => _showCreateHouseholdSheet(context),
              tooltip: 'Create household',
              icon: const Icon(Icons.add),
              label: const Text('Create'),
            ),
          );
        },
      ),
    );
  }

  Widget _buildBody(BuildContext context, HouseholdListViewModel viewModel) {
    if (viewModel.isLoading) {
      return const LoadingView(label: 'Loading households...');
    }

    if (viewModel.error != null) {
      return ErrorView(
        title: 'Could not load households',
        error: viewModel.error,
        stackTrace: viewModel.stackTrace,
        onRetry: viewModel.load,
      );
    }

    final List<Household> households = viewModel.households;
    if (households.isEmpty) {
      return EmptyView(
        title: 'You have not joined any households yet',
        message: 'Create a household to start organizing spaces and items.',
        action: FilledButton.icon(
          onPressed: () => _showCreateHouseholdSheet(context),
          icon: const Icon(Icons.add_home_outlined),
          label: const Text('Create household'),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: viewModel.load,
      child: ListView.separated(
        padding: const EdgeInsets.all(AppSpacing.lg),
        itemCount: households.length,
        separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.md),
        itemBuilder: (BuildContext context, int index) {
          final Household household = households[index];
          return AppCard(
            onTap: () {
              context.go(
                '/households/${household.id}/spaces?name=${Uri.encodeQueryComponent(household.name)}',
              );
            },
            child: Row(
              children: <Widget>[
                EntityAvatar(
                  id: household.id,
                  name: household.name,
                  size: EntityAvatarSize.lg,
                ),
                const SizedBox(width: AppSpacing.lg),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Text(
                        household.name,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: AppSpacing.sm),
                      Wrap(
                        spacing: AppSpacing.sm,
                        runSpacing: AppSpacing.xs,
                        children: <Widget>[
                          Chip(label: Text(household.subscriptionPlan)),
                          Chip(label: Text(household.subscriptionStatus)),
                        ],
                      ),
                    ],
                  ),
                ),
                const Icon(Icons.chevron_right),
              ],
            ),
          );
        },
      ),
    );
  }

  void _showCreateHouseholdSheet(BuildContext context) {
    final TextEditingController controller = TextEditingController();
    showModalBottomSheet<void>(
      context: context,
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
              Text(
                'Create household',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: AppSpacing.lg),
              TextField(
                controller: controller,
                decoration: const InputDecoration(
                  labelText: 'Name',
                  prefixIcon: Icon(Icons.home_outlined),
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Save'),
              ),
            ],
          ),
        );
      },
    ).whenComplete(controller.dispose);
  }
}
