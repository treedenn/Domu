import 'package:domu_mobile/features/auth/domain/auth_repository.dart';
import 'package:domu_mobile/features/auth/domain/auth_session.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final session = AuthSession(
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAt: DateTime.utc(2026, 1, 2),
  );

  test('reports cancelled sign-in without authenticating', () async {
    final controller = AuthViewModel(
      _FakeRepository(signInError: const AuthSignInCancelled()),
    );
    await controller.initialize();

    await controller.signIn(null);

    expect(controller.state, AuthSessionState.unauthenticated);
    expect(controller.signInMessage, 'Sign-in was cancelled.');
  });

  test('clears local credentials during sign-out', () async {
    final repository = _FakeRepository(restoredSession: session);
    final controller = AuthViewModel(repository);
    await controller.initialize();

    await controller.signOut();

    expect(repository.didSignOut, isTrue);
    expect(controller.state, AuthSessionState.unauthenticated);
  });

  test(
    'becomes unauthenticated when an access-token refresh is invalid',
    () async {
      final controller = AuthViewModel(
        _FakeRepository(restoredSession: session, invalidRefresh: true),
      );
      await controller.initialize();

      expect(await controller.validAccessToken(), isNull);
      expect(controller.state, AuthSessionState.unauthenticated);
    },
  );
}

class _FakeRepository implements AuthRepository {
  _FakeRepository({
    this.restoredSession,
    this.signInError,
    this.invalidRefresh = false,
  });

  final AuthSession? restoredSession;
  final Object? signInError;
  final bool invalidRefresh;
  bool didSignOut = false;

  @override
  Future<AuthSession?> refreshIfNeeded(AuthSession session) async =>
      invalidRefresh ? null : session;

  @override
  Future<AuthSession?> restoreSession() async => restoredSession;

  @override
  Future<AuthSession> signIn({String? loginHint}) async {
    if (signInError != null) throw signInError!;
    return restoredSession!;
  }

  @override
  Future<void> signOut() async => didSignOut = true;
}
