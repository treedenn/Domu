import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/auth/ui/login_view.dart';
import 'package:domu_mobile/features/auth/ui/splash_view.dart';
import 'package:domu_mobile/features/households/domain/household.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:domu_mobile/features/households/ui/households_view.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

abstract final class AppRoute {
  static const dashboard = '/';
  static const splash = '/splash';
  static const login = '/login';
  static const notFound = '/not-found';
}

class AppRouter {
  AppRouter(
    AuthViewModel authViewModel, {
    HouseholdsViewModel? householdsViewModel,
  }) : _householdsViewModel =
           householdsViewModel ??
           HouseholdsViewModel(_UnavailableHouseholdRepository()) {
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
          path: AppRoute.dashboard,
          builder: (_, _) => HouseholdsView(
            viewModel: _householdsViewModel,
            onSignOut: authViewModel.signOut,
          ),
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
