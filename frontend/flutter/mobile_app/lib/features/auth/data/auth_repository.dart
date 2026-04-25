import '../../../core/auth/auth_session.dart';

abstract class AuthRepository {
  Future<AuthSession?> restoreSession();

  Future<AuthSession> signIn({
    String? loginHint,
    String? preferredIdpId,
    bool createAccount = false,
  });

  Future<void> signOut(AuthSession? session);
}
