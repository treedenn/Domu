import '../domain/auth_session.dart';

class OidcTokens {
  const OidcTokens({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
  });

  final String? accessToken;
  final String? refreshToken;
  final DateTime? expiresAt;

  AuthSession toSession() {
    final accessToken = this.accessToken;
    final refreshToken = this.refreshToken;
    final expiresAt = this.expiresAt;
    if (accessToken == null || refreshToken == null || expiresAt == null) {
      throw StateError(
        'Zitadel did not return a renewable access-token session.',
      );
    }
    return AuthSession(
      accessToken: accessToken,
      refreshToken: refreshToken,
      expiresAt: expiresAt.toUtc(),
    );
  }
}

abstract interface class OidcClient {
  Future<OidcTokens> authorize({String? loginHint});
  Future<OidcTokens> refresh(String refreshToken);
}
