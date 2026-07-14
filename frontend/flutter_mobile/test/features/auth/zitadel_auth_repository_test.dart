import 'package:domu_mobile/features/auth/data/oidc_client.dart';
import 'package:domu_mobile/features/auth/data/session_storage.dart';
import 'package:domu_mobile/features/auth/data/zitadel_auth_repository.dart';
import 'package:domu_mobile/features/auth/domain/auth_session.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final now = DateTime.utc(2026, 1, 1, 12);

  test('restores a valid persisted session without a refresh', () async {
    final oidc = _FakeOidcClient();
    final storage = _MemoryStorage(_session(now.add(const Duration(hours: 1))));
    final repository = ZitadelAuthRepository(oidc, storage, clock: () => now);

    final session = await repository.restoreSession();

    expect(session?.accessToken, 'access');
    expect(oidc.refreshes, isEmpty);
  });

  test(
    'refreshes an expired persisted session and saves the replacement',
    () async {
      final oidc = _FakeOidcClient(
        refreshResponse: OidcTokens(
          accessToken: 'new-access',
          refreshToken: 'new-refresh',
          expiresAt: now.add(const Duration(hours: 1)),
        ),
      );
      final storage = _MemoryStorage(_session(now));
      final repository = ZitadelAuthRepository(oidc, storage, clock: () => now);

      final session = await repository.restoreSession();

      expect(session?.accessToken, 'new-access');
      expect(storage.value?.refreshToken, 'new-refresh');
    },
  );

  test('clears an invalid session when refresh fails', () async {
    final storage = _MemoryStorage(_session(now));
    final repository = ZitadelAuthRepository(
      _FakeOidcClient(refreshError: StateError('invalid_grant')),
      storage,
      clock: () => now,
    );

    expect(await repository.restoreSession(), isNull);
    expect(storage.wasCleared, isTrue);
  });

  test('persists a successful sign-in and forwards its login hint', () async {
    final oidc = _FakeOidcClient(
      authorizationResponse: OidcTokens(
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: now.add(const Duration(hours: 1)),
      ),
    );
    final storage = _MemoryStorage(null);
    final repository = ZitadelAuthRepository(oidc, storage, clock: () => now);

    await repository.signIn(loginHint: 'hello@example.com');

    expect(oidc.loginHint, 'hello@example.com');
    expect(storage.value?.accessToken, 'access');
  });
}

AuthSession _session(DateTime expiresAt) => AuthSession(
  accessToken: 'access',
  refreshToken: 'refresh',
  expiresAt: expiresAt,
);

class _MemoryStorage implements SessionStorage {
  _MemoryStorage(this.value);

  AuthSession? value;
  bool wasCleared = false;

  @override
  Future<void> clear() async {
    wasCleared = true;
    value = null;
  }

  @override
  Future<AuthSession?> read() async => value;

  @override
  Future<void> write(AuthSession session) async => value = session;
}

class _FakeOidcClient implements OidcClient {
  _FakeOidcClient({
    this.authorizationResponse,
    this.refreshResponse,
    this.refreshError,
  });

  final OidcTokens? authorizationResponse;
  final OidcTokens? refreshResponse;
  final Object? refreshError;
  final List<String> refreshes = [];
  String? loginHint;

  @override
  Future<OidcTokens> authorize({String? loginHint}) async {
    this.loginHint = loginHint;
    return authorizationResponse ??
        const OidcTokens(
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
        );
  }

  @override
  Future<OidcTokens> refresh(String refreshToken) async {
    refreshes.add(refreshToken);
    if (refreshError != null) throw refreshError!;
    return refreshResponse!;
  }
}
