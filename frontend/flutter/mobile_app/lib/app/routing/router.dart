import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/ui/loading_view.dart';
import '../../features/auth/presentation/controllers/auth_controller.dart';
import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/households/data/households_repository.dart';
import '../../features/households/presentation/screens/household_list_screen.dart';
import '../../features/households/presentation/screens/household_settings_screen.dart';
import '../../features/households/presentation/screens/members_tab_screen.dart';
import '../../features/households/data/members_repository.dart';
import '../../features/items/data/items_repository.dart';
import '../../features/search/data/search_repository.dart';
import '../../features/items/presentation/screens/entry_editor_screen.dart';
import '../../features/items/presentation/screens/item_detail_screen.dart';
import '../../features/search/presentation/screens/search_screen.dart';
import '../../features/spaces/data/spaces_repository.dart';
import '../../features/spaces/presentation/screens/space_detail_screen.dart';
import '../../features/spaces/presentation/screens/space_list_screen.dart';
import '../bootstrap/app_config.dart';
import '../shell/app_shell.dart';

GoRouter buildRouter(AuthController authController) {
  return GoRouter(
    initialLocation: '/households',
    refreshListenable: authController,
    redirect: (BuildContext context, GoRouterState state) {
      final bool initializing = authController.state.isInitializing;
      final bool loggedIn = authController.state.isAuthenticated;
      final bool atLogin = state.matchedLocation == '/login';

      if (initializing) {
        return null;
      }
      if (!loggedIn && !atLogin) {
        return '/login';
      }
      if (loggedIn && atLogin) {
        return '/households';
      }
      return null;
    },
    routes: <RouteBase>[
      GoRoute(
        path: '/login',
        builder: (BuildContext context, GoRouterState state) => LoginScreen(
          controller: context.read<AuthController>(),
          config: context.read<AppConfig>(),
        ),
      ),
      GoRoute(
        path: '/households',
        builder: (BuildContext context, GoRouterState state) {
          final AuthController authController = context.read<AuthController>();
          final authState = authController.state;
          if (authState.isInitializing) {
            return const Scaffold(
              body: LoadingView(label: 'Restoring session...'),
            );
          }
          return HouseholdListScreen(
            repository: context.read<HouseholdsRepository>(),
            session: authState.session,
            onSignOut: authController.signOut,
          );
        },
      ),
      ShellRoute(
        builder: (BuildContext context, GoRouterState state, Widget child) {
          return AppShell(
            householdId: state.pathParameters['hid']!,
            householdName: state.uri.queryParameters['name'] ?? 'Household',
            child: child,
          );
        },
        routes: <RouteBase>[
          GoRoute(
            path: '/households/:hid/spaces',
            builder: (BuildContext context, GoRouterState state) =>
                SpaceListScreen(
                  householdId: state.pathParameters['hid']!,
                  householdName:
                      state.uri.queryParameters['name'] ?? 'Household',
                  repository: context.read<SpacesRepository>(),
                  itemsRepository: context.read<ItemsRepository>(),
                  session: context.read<AuthController>().state.session,
                ),
            routes: <RouteBase>[
              GoRoute(
                path: ':sid',
                builder: (BuildContext context, GoRouterState state) =>
                    SpaceDetailScreen(
                      householdId: state.pathParameters['hid']!,
                      householdName:
                          state.uri.queryParameters['name'] ?? 'Household',
                      spaceId: state.pathParameters['sid']!,
                      spacesRepository: context.read<SpacesRepository>(),
                      itemsRepository: context.read<ItemsRepository>(),
                      session: context.read<AuthController>().state.session,
                    ),
                routes: <RouteBase>[
                  GoRoute(
                    path: 'items/:iid',
                    builder: (BuildContext context, GoRouterState state) =>
                        ItemDetailScreen(
                          householdId: state.pathParameters['hid']!,
                          spaceId: state.pathParameters['sid']!,
                          itemId: state.pathParameters['iid']!,
                          repository: context.read<ItemsRepository>(),
                          session: context.read<AuthController>().state.session,
                        ),
                    routes: <RouteBase>[
                      GoRoute(
                        path: 'entries/new',
                        builder: (BuildContext context, GoRouterState state) =>
                            EntryEditorScreen(
                              householdId: state.pathParameters['hid']!,
                              spaceId: state.pathParameters['sid']!,
                              itemId: state.pathParameters['iid']!,
                              repository: context.read<ItemsRepository>(),
                              session: context
                                  .read<AuthController>()
                                  .state
                                  .session,
                            ),
                      ),
                      GoRoute(
                        path: 'entries/:eid',
                        builder: (BuildContext context, GoRouterState state) =>
                            EntryEditorScreen(
                              householdId: state.pathParameters['hid']!,
                              spaceId: state.pathParameters['sid']!,
                              itemId: state.pathParameters['iid']!,
                              entryId: state.pathParameters['eid'],
                              repository: context.read<ItemsRepository>(),
                              session: context
                                  .read<AuthController>()
                                  .state
                                  .session,
                            ),
                      ),
                    ],
                  ),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/households/:hid/members',
            builder: (BuildContext context, GoRouterState state) =>
                MembersTabScreen(
                  householdId: state.pathParameters['hid']!,
                  repository: context.read<MembersRepository>(),
                  session: context.read<AuthController>().state.session,
                ),
          ),
          GoRoute(
            path: '/households/:hid/search',
            builder: (BuildContext context, GoRouterState state) =>
                SearchScreen(
                  householdId: state.pathParameters['hid']!,
                  repository: context.read<SearchRepository>(),
                  session: context.read<AuthController>().state.session,
                  initialExpiringFilter: state.uri.queryParameters['expiring'],
                ),
          ),
          GoRoute(
            path: '/households/:hid/settings',
            builder: (BuildContext context, GoRouterState state) =>
                HouseholdSettingsScreen(
                  householdId: state.pathParameters['hid']!,
                  householdName:
                      state.uri.queryParameters['name'] ?? 'Household',
                  onSignOut: context.read<AuthController>().signOut,
                ),
          ),
          GoRoute(
            path: '/households/:hid/shopping',
            builder: (BuildContext context, GoRouterState state) =>
                const Placeholder(),
          ),
        ],
      ),
    ],
  );
}
