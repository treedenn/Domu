import 'auth_session.dart';

abstract interface class AuthRepository {
  Future<AuthSession?> restoreSession();
  Future<AuthSession> signIn({String? loginHint});
  Future<AuthSession?> refreshIfNeeded(AuthSession session);
  Future<void> signOut();
}

class AuthSignInCancelled implements Exception {
  const AuthSignInCancelled();
}
