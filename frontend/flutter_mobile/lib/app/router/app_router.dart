import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/auth/ui/login_view.dart';
import 'package:domu_mobile/features/auth/ui/splash_view.dart';
import 'package:domu_mobile/features/dashboard/ui/dashboard_view.dart';
import 'package:domu_mobile/features/households/domain/household.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:domu_mobile/features/households/ui/household_shell.dart';
import 'package:domu_mobile/features/households/ui/households_view.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:domu_mobile/features/members/ui/members_view.dart';
import 'package:domu_mobile/features/members/domain/household_member.dart';
import 'package:domu_mobile/features/members/domain/members_repository.dart';
import 'package:domu_mobile/features/members/domain/members_result.dart';
import 'package:domu_mobile/features/members/domain/pending_invitation.dart';
import 'package:domu_mobile/features/members/ui/members_view_model.dart';
import 'package:domu_mobile/features/shopping_lists/ui/shopping_lists_view.dart';
import 'package:domu_mobile/features/spaces/ui/spaces_view.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

abstract final class AppRoute {
  static const households = '/';
  static const dashboard = households;
  static const splash = '/splash';
  static const login = '/login';
  static const notFound = '/not-found';
}

class AppRouter {
  AppRouter(
    AuthViewModel authViewModel, {
    HouseholdsViewModel? householdsViewModel,
    MembersViewModel? membersViewModel,
  }) : _householdsViewModel =
           householdsViewModel ??
           HouseholdsViewModel(_UnavailableHouseholdRepository()) {
    _membersViewModel =
        membersViewModel ?? MembersViewModel(_UnavailableMembersRepository());
    router = GoRouter(
      initialLocation: AppRoute.dashboard,
      refreshListenable: authViewModel,
      redirect: (context, state) => _redirect(authViewModel, state),
      routes: [
        GoRoute(path: AppRoute.splash, builder: (_, _) => const SplashView()),
        GoRoute(
          path: AppRoute.login,
          builder: (_, _) => LoginView(viewModel: authViewModel),
        ),
        GoRoute(
          path: AppRoute.notFound,
          builder: (_, _) => const UnknownRouteScreen(),
        ),
        GoRoute(
          path: AppRoute.households,
          builder: (_, _) => HouseholdsView(
            viewModel: _householdsViewModel,
            onSignOut: authViewModel.signOut,
            onHouseholdSelected: (household) =>
                router.go('/households/${household.id}/dashboard'),
          ),
        ),
        ShellRoute(
          builder: (_, state, child) => HouseholdShell(
            key: ValueKey(state.pathParameters['householdId']),
            householdId: state.pathParameters['householdId']!,
            viewModel: _householdsViewModel,
            onSignOut: authViewModel.signOut,
            child: child,
          ),
          routes: [
            GoRoute(
              path: '/households/:householdId/dashboard',
              builder: (_, _) => const DashboardView(),
            ),
            GoRoute(
              path: '/households/:householdId/members',
              builder: (_, state) => MembersView(
                householdId: state.pathParameters['householdId']!,
                viewModel: _membersViewModel,
              ),
            ),
            GoRoute(
              path: '/households/:householdId/shopping-lists',
              builder: (_, _) => const ShoppingListsView(),
            ),
            GoRoute(
              path: '/households/:householdId/spaces',
              builder: (_, _) => const SpacesView(),
            ),
          ],
        ),
        GoRoute(
          path: '/:unknown(.*)',
          builder: (_, _) => const UnknownRouteScreen(),
        ),
      ],
    );
  }

  late final GoRouter router;
  final HouseholdsViewModel _householdsViewModel;
  late final MembersViewModel _membersViewModel;

  static String? _redirect(AuthViewModel auth, GoRouterState state) {
    final location = state.uri.toString();
    final isInitializing = auth.state == AuthSessionState.initializing;
    final isAuthenticated = auth.state == AuthSessionState.authenticated;
    final isSplash = state.uri.path == AppRoute.splash;
    final isLogin = state.uri.path == AppRoute.login;
    final isPublic = isSplash || isLogin || state.uri.path == AppRoute.notFound;

    if (isInitializing) {
      return isSplash ? null : _withFrom(AppRoute.splash, location);
    }

    if (isSplash) {
      final from = _safeFrom(state.uri.queryParameters['from']);
      return isAuthenticated
          ? (from ?? AppRoute.dashboard)
          : _withFrom(AppRoute.login, from);
    }

    if (!isAuthenticated && !isPublic) {
      return _withFrom(AppRoute.login, location);
    }

    if (isAuthenticated && isLogin) {
      return _safeFrom(state.uri.queryParameters['from']) ?? AppRoute.dashboard;
    }
    return null;
  }

  static String _withFrom(String path, String? from) {
    if (from == null || from.isEmpty || from == AppRoute.dashboard) return path;
    return Uri(
      path: path,
      queryParameters: <String, String>{'from': from},
    ).toString();
  }

  static String? _safeFrom(String? value) {
    if (value == null || !value.startsWith('/') || value.startsWith('//')) {
      return null;
    }
    final uri = Uri.tryParse(value);
    return uri != null && !uri.hasScheme && uri.host.isEmpty ? value : null;
  }
}

class _UnavailableMembersRepository implements MembersRepository {
  @override
  Future<void> archiveMember({
    required String householdId,
    required HouseholdMember member,
  }) => Future<void>.error(
    const MembersRepositoryException('Members are unavailable.'),
  );

  @override
  Future<PendingInvitation> createInvitation({
    required String householdId,
    required String displayName,
    required String email,
    required HouseholdMemberRole role,
  }) => Future<PendingInvitation>.error(
    const MembersRepositoryException('Members are unavailable.'),
  );

  @override
  Future<MembersResult> getMembers(String householdId) =>
      Future<MembersResult>.error(
        const MembersRepositoryException('Members are unavailable.'),
      );

  @override
  Future<List<PendingInvitation>> getPendingInvitations(String householdId) =>
      Future<List<PendingInvitation>>.error(
        const MembersRepositoryException('Members are unavailable.'),
      );
}

class _UnavailableHouseholdRepository implements HouseholdRepository {
  @override
  Future<Never> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) => Future<Never>.error(
    const HouseholdRepositoryException('Households are unavailable.'),
  );

  @override
  Future<void> deleteHousehold(String id) => Future<void>.error(
    const HouseholdRepositoryException('Households are unavailable.'),
  );

  @override
  Future<List<Household>> getHouseholds() => Future<List<Household>>.error(
    const HouseholdRepositoryException('Households are unavailable.'),
  );

  @override
  Future<Never> updateHousehold({required String id, required String name}) =>
      Future<Never>.error(
        const HouseholdRepositoryException('Households are unavailable.'),
      );
}

class UnknownRouteScreen extends StatelessWidget {
  const UnknownRouteScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(body: Center(child: Text('Page not found')));
  }
}
