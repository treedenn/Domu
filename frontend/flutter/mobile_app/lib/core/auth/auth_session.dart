class AuthSession {
  const AuthSession({
    required this.accessToken,
    required this.expiresAt,
    this.idToken,
    this.refreshToken,
    this.tokenType,
    this.userId,
  });

  final String accessToken;
  final DateTime expiresAt;
  final String? idToken;
  final String? refreshToken;
  final String? tokenType;
  final String? userId;

  bool get isExpired => DateTime.now().isAfter(expiresAt);

  AuthSession copyWith({
    String? accessToken,
    DateTime? expiresAt,
    String? idToken,
    String? refreshToken,
    String? tokenType,
    String? userId,
  }) {
    return AuthSession(
      accessToken: accessToken ?? this.accessToken,
      expiresAt: expiresAt ?? this.expiresAt,
      idToken: idToken ?? this.idToken,
      refreshToken: refreshToken ?? this.refreshToken,
      tokenType: tokenType ?? this.tokenType,
      userId: userId ?? this.userId,
    );
  }
}
