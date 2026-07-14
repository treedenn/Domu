import 'package:domu_mobile/app/router/app_router.dart';
import 'package:domu_mobile/features/auth/domain/auth_repository.dart';
import 'package:domu_mobile/features/auth/domain/auth_session.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/auth/ui/splash_view.dart';
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
