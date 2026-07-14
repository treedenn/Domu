import '../domain/auth_repository.dart';
import '../domain/auth_session.dart';
import 'oidc_client.dart';
import 'session_storage.dart';

class ZitadelAuthRepository implements AuthRepository {
  ZitadelAuthRepository(
    this._oidcClient,
    this._storage, {
    DateTime Function()? clock,
  }) : _clock = clock ?? DateTime.now;

  static const _refreshLeadTime = Duration(minutes: 1);
  final OidcClient _oidcClient;
  final SessionStorage _storage;
  final DateTime Function() _clock;

  @override
  Future<AuthSession?> restoreSession() async {
    final session = await _storage.read();
    if (session == null) {
      return null;
    }
    return refreshIfNeeded(session);
  }

  @override
  Future<AuthSession> signIn({String? loginHint}) async {
    final session = (await _oidcClient.authorize(
      loginHint: loginHint,
    )).toSession();
    await _storage.write(session);
    return session;
  }

  @override
  Future<AuthSession?> refreshIfNeeded(AuthSession session) async {
    if (!session.expiresWithin(_refreshLeadTime, _clock().toUtc())) {
      return session;
    }
    try {
      final refreshed = (await _oidcClient.refresh(
        session.refreshToken,
      )).toSession();
      await _storage.write(refreshed);
      return refreshed;
    } catch (_) {
      await _storage.clear();
      return null;
    }
  }

  @override
  Future<void> signOut() => _storage.clear();
}
