import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../app/theme/tokens.dart';
import '../../shared/widgets/breadcrumbs.dart';

const bool kEnableShopping = false;

class AppShell extends StatelessWidget {
  const AppShell({
    required this.householdId,
    required this.householdName,
    required this.child,
    super.key,
  });

  final String householdId;
  final String householdName;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final int index = _indexFor(GoRouterState.of(context).uri.path);

    return Scaffold(
      appBar: AppBar(
        title: Text(householdName),
        actions: <Widget>[
          IconButton(
            tooltip: 'Search',
            onPressed: () => context.go(_branchPath(context, 'search')),
            icon: const Icon(Icons.search),
          ),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(44),
          child: Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.lg,
              0,
              AppSpacing.lg,
              AppSpacing.sm,
            ),
            child: Breadcrumbs(householdName: householdName),
          ),
        ),
      ),
      body: child,
      bottomNavigationBar: NavigationBar(
        selectedIndex: index,
        onDestinationSelected: (int selected) {
          final List<String> branches = <String>[
            'spaces',
            'members',
            if (kEnableShopping) 'shopping',
            'search',
            'settings',
          ];
          context.go(_branchPath(context, branches[selected]));
        },
        destinations: <NavigationDestination>[
          const NavigationDestination(
            icon: Icon(Icons.inventory_2_outlined),
            selectedIcon: Icon(Icons.inventory_2),
            label: 'Spaces',
          ),
          const NavigationDestination(
            icon: Icon(Icons.group_outlined),
            selectedIcon: Icon(Icons.group),
            label: 'Members',
          ),
          if (kEnableShopping)
            const NavigationDestination(
              icon: Icon(Icons.shopping_bag_outlined),
              selectedIcon: Icon(Icons.shopping_bag),
              label: 'Shopping',
            ),
          const NavigationDestination(
            icon: Icon(Icons.search),
            label: 'Search',
          ),
          const NavigationDestination(
            icon: Icon(Icons.settings_outlined),
            selectedIcon: Icon(Icons.settings),
            label: 'Settings',
          ),
        ],
      ),
    );
  }

  String _branchPath(BuildContext context, String branch) {
    final Uri uri = GoRouterState.of(context).uri;
    final String encodedName = Uri.encodeQueryComponent(householdName);
    return '/households/$householdId/$branch?name=$encodedName${uri.queryParameters.containsKey('expiring') ? '&expiring=${uri.queryParameters['expiring']}' : ''}';
  }

  int _indexFor(String path) {
    if (path.contains('/members')) {
      return 1;
    }
    if (kEnableShopping && path.contains('/shopping')) {
      return 2;
    }
    if (path.contains('/search')) {
      return kEnableShopping ? 3 : 2;
    }
    if (path.contains('/settings')) {
      return kEnableShopping ? 4 : 3;
    }
    return 0;
  }
}
