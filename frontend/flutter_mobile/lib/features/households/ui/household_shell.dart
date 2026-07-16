import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../domain/household.dart';
import 'households_view_model.dart';

enum HouseholdSection { dashboard, members, shoppingLists, spaces }

class HouseholdShell extends StatefulWidget {
  const HouseholdShell({
    super.key,
    required this.householdId,
    required this.viewModel,
    required this.child,
    this.onSignOut,
  });

  final String householdId;
  final HouseholdsViewModel viewModel;
  final Widget child;
  final Future<void> Function()? onSignOut;

  @override
  State<HouseholdShell> createState() => _HouseholdShellState();
}

class _HouseholdShellState extends State<HouseholdShell> {
  bool _hasResolved = false;
  bool _hasResolvedHousehold = false;
  bool _waitingForInitialLoad = false;

  @override
  void initState() {
    super.initState();
    if (widget.viewModel.households.isEmpty) {
      _waitingForInitialLoad = true;
      WidgetsBinding.instance.addPostFrameCallback((_) async {
        await widget.viewModel.load();
        if (mounted) setState(() => _waitingForInitialLoad = false);
      });
    }
  }

  @override
  void didUpdateWidget(covariant HouseholdShell oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.householdId != widget.householdId) {
      _hasResolved = false;
      _hasResolvedHousehold = false;
    }
  }

  void _handleMissingHousehold() {
    if (_hasResolved ||
        widget.viewModel.isLoading ||
        widget.viewModel.isRefreshing) {
      return;
    }
    _hasResolved = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      context.go(_hasResolvedHousehold ? '/' : '/not-found');
    });
  }

  @override
  Widget build(BuildContext context) {
    final section = _sectionForLocation(GoRouterState.of(context).uri.path);
    return AnimatedBuilder(
      animation: widget.viewModel,
      builder: (context, _) {
        if (_waitingForInitialLoad || widget.viewModel.isLoading) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }
        final household = widget.viewModel.households
            .cast<Household?>()
            .firstWhere(
              (candidate) => candidate?.id == widget.householdId,
              orElse: () => null,
            );
        if (household == null) {
          _handleMissingHousehold();
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }
        _hasResolved = false;
        _hasResolvedHousehold = true;
        if (widget.viewModel.selectedHouseholdId != household.id) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            if (mounted) widget.viewModel.selectHousehold(household);
          });
        }
        return Scaffold(
          appBar: AppBar(
            title: _HouseholdSelector(
              household: household,
              households: widget.viewModel.households,
              onSelected: (selected) {
                widget.viewModel.selectHousehold(selected);
                context.go('/households/${selected.id}/dashboard');
              },
              onManage: () => context.go('/'),
            ),
            actions: [
              IconButton(
                tooltip: 'Sign out',
                onPressed: widget.onSignOut,
                icon: const Icon(Icons.logout),
              ),
            ],
          ),
          body: widget.child,
          bottomNavigationBar: NavigationBar(
            selectedIndex: section.index,
            onDestinationSelected: (index) => context.go(
              '/households/${household.id}/${HouseholdSection.values[index].path}',
            ),
            destinations: const [
              NavigationDestination(
                icon: Icon(Icons.dashboard_outlined),
                selectedIcon: Icon(Icons.dashboard),
                label: 'Dashboard',
              ),
              NavigationDestination(
                icon: Icon(Icons.people_outline),
                selectedIcon: Icon(Icons.people),
                label: 'Members',
              ),
              NavigationDestination(
                icon: Icon(Icons.shopping_cart_outlined),
                selectedIcon: Icon(Icons.shopping_cart),
                label: 'Shopping Lists',
              ),
              NavigationDestination(
                icon: Icon(Icons.room_outlined),
                selectedIcon: Icon(Icons.room),
                label: 'Spaces',
              ),
            ],
          ),
        );
      },
    );
  }
}

extension on HouseholdSection {
  String get path => switch (this) {
    HouseholdSection.dashboard => 'dashboard',
    HouseholdSection.members => 'members',
    HouseholdSection.shoppingLists => 'shopping-lists',
    HouseholdSection.spaces => 'spaces',
  };
}

HouseholdSection _sectionForLocation(String location) =>
    HouseholdSection.values.firstWhere(
      (section) =>
          location.endsWith('/${section.path}') ||
          (section == HouseholdSection.shoppingLists &&
              location.contains('/shopping-lists/')),
      orElse: () => HouseholdSection.dashboard,
    );

class _HouseholdSelector extends StatelessWidget {
  const _HouseholdSelector({
    required this.household,
    required this.households,
    required this.onSelected,
    required this.onManage,
  });

  final Household household;
  final List<Household> households;
  final ValueChanged<Household> onSelected;
  final VoidCallback onManage;

  @override
  Widget build(BuildContext context) => PopupMenuButton<Household?>(
    tooltip: 'Select household',
    onSelected: (selected) =>
        selected == null ? onManage() : onSelected(selected),
    itemBuilder: (context) => [
      ...households.map(
        (candidate) =>
            PopupMenuItem(value: candidate, child: Text(candidate.name)),
      ),
      const PopupMenuDivider(),
      const PopupMenuItem<Household?>(
        value: null,
        child: Text('Manage households'),
      ),
    ],
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [Text(household.name), const Icon(Icons.arrow_drop_down)],
    ),
  );
}
