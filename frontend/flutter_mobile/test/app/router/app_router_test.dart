import 'package:domu_mobile/app/router/app_router.dart';
import 'package:domu_mobile/features/auth/domain/auth_repository.dart';
import 'package:domu_mobile/features/auth/domain/auth_session.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/auth/ui/splash_view.dart';
import 'package:domu_mobile/features/households/domain/household.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:domu_mobile/features/households/ui/household_shell.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets(
    'holds at splash, then redirects protected routes through login',
    (tester) async {
      final controller = AuthViewModel(_RouterRepository());
      final router = AppRouter(controller).router;
      await tester.pumpWidget(MaterialApp.router(routerConfig: router));
      expect(find.byType(SplashView), findsOneWidget);

      await controller.initialize();
      await tester.pumpAndSettle();
      expect(find.text('Welcome home'), findsOneWidget);
    },
  );

  testWidgets('returns an authenticated user to the requested unknown route', (
    tester,
  ) async {
    final repository = _RouterRepository();
    final controller = AuthViewModel(repository);
    final router = AppRouter(controller).router;
    await tester.pumpWidget(MaterialApp.router(routerConfig: router));
    await controller.initialize();
    await tester.pumpAndSettle();

    // An unknown route is still protected, so it is preserved through login.
    router.go('/does-not-exist?tab=activity');
    await tester.pumpAndSettle();
    expect(find.text('Welcome home'), findsOneWidget);
    await controller.signIn(null);
    await tester.pumpAndSettle();
    expect(find.text('Page not found'), findsOneWidget);
  });

  testWidgets('selection and bottom navigation retain the household URL', (
    tester,
  ) async {
    final controller = AuthViewModel(_RouterRepository());
    final router = AppRouter(
      controller,
      householdsViewModel: HouseholdsViewModel(_HouseholdRepository()),
    ).router;
    await tester.pumpWidget(MaterialApp.router(routerConfig: router));
    await controller.initialize();
    await tester.pumpAndSettle();
    await controller.signIn(null);
    await tester.pumpAndSettle();

    await tester.tap(find.text('Home'));
    await tester.pumpAndSettle();
    expect(
      router.routeInformationProvider.value.uri.path,
      '/households/home/dashboard',
    );
    await tester.tap(find.text('Members'));
    await tester.pumpAndSettle();
    expect(
      router.routeInformationProvider.value.uri.path,
      '/households/home/members',
    );
  });

  testWidgets('selector switches households and scoped routes validate IDs', (
    tester,
  ) async {
    final controller = AuthViewModel(_RouterRepository());
    final router = AppRouter(
      controller,
      householdsViewModel: HouseholdsViewModel(_HouseholdRepository()),
    ).router;
    await tester.pumpWidget(MaterialApp.router(routerConfig: router));
    await controller.initialize();
    await tester.pumpAndSettle();
    await controller.signIn(null);
    await tester.pumpAndSettle();
    router.go('/households/home/dashboard');
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Select household'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Cabin').last);
    await tester.pumpAndSettle();
    expect(
      router.routeInformationProvider.value.uri.path,
      '/households/cabin/dashboard',
    );

    router.go('/households/missing/spaces');
    await tester.pumpAndSettle();
    expect(find.text('Page not found'), findsOneWidget);
  });

  testWidgets('login returns to the requested household route', (tester) async {
    final controller = AuthViewModel(_RouterRepository());
    final router = AppRouter(
      controller,
      householdsViewModel: HouseholdsViewModel(_HouseholdRepository()),
    ).router;
    await tester.pumpWidget(MaterialApp.router(routerConfig: router));
    await controller.initialize();
    await tester.pumpAndSettle();

    router.go('/households/home/spaces');
    await tester.pumpAndSettle();
    expect(find.text('Welcome home'), findsOneWidget);
    await controller.signIn(null);
    await tester.pumpAndSettle();
    expect(
      router.routeInformationProvider.value.uri.path,
      '/households/home/spaces',
    );
    expect(find.byType(HouseholdShell), findsOneWidget);
  });
}

class _RouterRepository implements AuthRepository {
  AuthSession? _session;

  @override
  Future<AuthSession?> refreshIfNeeded(AuthSession session) async => session;

  @override
  Future<AuthSession?> restoreSession() async => _session;

  @override
  Future<AuthSession> signIn({String? loginHint}) async =>
      _session = AuthSession(
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: DateTime.utc(2027),
      );

  @override
  Future<void> signOut() async => _session = null;
}

class _HouseholdRepository implements HouseholdRepository {
  static const _households = [
    Household(
      id: 'home',
      name: 'Home',
      subscriptionPlan: HouseholdSubscriptionPlan.free,
      subscriptionStatus: HouseholdSubscriptionStatus.active,
    ),
    Household(
      id: 'cabin',
      name: 'Cabin',
      subscriptionPlan: HouseholdSubscriptionPlan.free,
      subscriptionStatus: HouseholdSubscriptionStatus.active,
    ),
  ];

  @override
  Future<Household> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) => throw UnimplementedError();

  @override
  Future<void> deleteHousehold(String id) => throw UnimplementedError();

  @override
  Future<List<Household>> getHouseholds() async => _households;

  @override
  Future<Household> updateHousehold({
    required String id,
    required String name,
  }) => throw UnimplementedError();
}
