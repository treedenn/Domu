import 'package:flutter_appauth/flutter_appauth.dart';

import '../../../app/bootstrap/app_config.dart';
import '../../../core/auth/auth_session.dart';
import '../../../core/storage/secure_store.dart';
import 'auth_repository.dart';

class ZitadelAuthRepository implements AuthRepository {
  ZitadelAuthRepository({
    required FlutterAppAuth appAuth,
    required AppConfig config,
    required SecureStore secureStore,
  })  : _appAuth = appAuth,
        _config = config,
        _secureStore = secureStore;

  static const String _accessTokenKey = 'auth.access_token';
  static const String _refreshTokenKey = 'auth.refresh_token';
  static const String _idTokenKey = 'auth.id_token';
  static const String _expiresAtKey = 'auth.expires_at';
  static const String _tokenTypeKey = 'auth.token_type';

  final FlutterAppAuth _appAuth;
  final AppConfig _config;
  final SecureStore _secureStore;

  @override
  Future<AuthSession?> restoreSession() async {
    final AuthSession? storedSession = await _readSession();
    if (storedSession == null) {
      return null;
    }

    if (!storedSession.isExpired) {
      return storedSession;
    }

    if (storedSession.refreshToken == null || storedSession.refreshToken!.isEmpty) {
      await _clearSession();
      return null;
    }

    try {
      final TokenResponse response = await _appAuth.token(
        TokenRequest(
          _config.oidcClientId,
          _config.oidcRedirectUri,
          issuer: _config.oidcIssuer,
          refreshToken: storedSession.refreshToken,
          scopes: _config.authScopes,
          allowInsecureConnections: _config.allowInsecureConnections,
        ),
      );

      return _persistTokenResponse(response, fallback: storedSession);
    } catch (_) {
      await _clearSession();
      return null;
    }
  }

  @override
  Future<AuthSession> signIn({
    String? loginHint,
    String? preferredIdpId,
    bool createAccount = false,
  }) async {
    final List<String> scopes = <String>[
      ..._config.authScopes,
      if (_hasValue(preferredIdpId))
        'urn:zitadel:iam:org:idp:id:${preferredIdpId!.trim()}',
    ];

    final AuthorizationTokenResponse response =
        await _appAuth.authorizeAndExchangeCode(
      AuthorizationTokenRequest(
        _config.oidcClientId,
        _config.oidcRedirectUri,
        issuer: _config.oidcIssuer,
        scopes: scopes,
        loginHint: _normalized(loginHint),
        promptValues: createAccount ? const <String>['create'] : null,
        allowInsecureConnections: _config.allowInsecureConnections,
      ),
    );

    if (response.accessToken == null) {
      throw const AuthRepositoryException('Authentication did not return an access token.');
    }

    return _persistAuthorizationResponse(response);
  }

  @override
  Future<void> signOut(AuthSession? session) async {
    if (session?.idToken != null) {
      try {
        await _appAuth.endSession(
          EndSessionRequest(
            idTokenHint: session!.idToken,
            postLogoutRedirectUrl: _config.oidcRedirectUri,
            issuer: _config.oidcIssuer,
            allowInsecureConnections: _config.allowInsecureConnections,
          ),
        );
      } catch (_) {
        // Clearing local credentials is still preferable to blocking sign-out.
      }
    }

    await _clearSession();
  }

  Future<AuthSession?> _readSession() async {
    final String? accessToken = await _secureStore.read(key: _accessTokenKey);
    final String? expiresAtRaw = await _secureStore.read(key: _expiresAtKey);
    if (accessToken == null || expiresAtRaw == null) {
      return null;
    }

    final DateTime? expiresAt = DateTime.tryParse(expiresAtRaw);
    if (expiresAt == null) {
      await _clearSession();
      return null;
    }

    return AuthSession(
      accessToken: accessToken,
      expiresAt: expiresAt,
      idToken: await _secureStore.read(key: _idTokenKey),
      refreshToken: await _secureStore.read(key: _refreshTokenKey),
      tokenType: await _secureStore.read(key: _tokenTypeKey),
    );
  }

  Future<AuthSession> _persistAuthorizationResponse(
    AuthorizationTokenResponse response,
  ) {
    final AuthSession session = AuthSession(
      accessToken: response.accessToken!,
      expiresAt: response.accessTokenExpirationDateTime ?? _defaultExpiry(),
      idToken: response.idToken,
      refreshToken: response.refreshToken,
      tokenType: response.tokenType,
    );

    return _persistSession(session);
  }

  Future<AuthSession> _persistTokenResponse(
    TokenResponse response, {
    required AuthSession fallback,
  }) {
    final String? accessToken = response.accessToken;
    if (accessToken == null) {
      throw const AuthRepositoryException('Token refresh did not return an access token.');
    }

    final AuthSession session = fallback.copyWith(
      accessToken: accessToken,
      expiresAt: response.accessTokenExpirationDateTime ?? _defaultExpiry(),
      idToken: response.idToken ?? fallback.idToken,
      refreshToken: response.refreshToken ?? fallback.refreshToken,
      tokenType: response.tokenType ?? fallback.tokenType,
    );

    return _persistSession(session);
  }

  Future<AuthSession> _persistSession(AuthSession session) async {
    await _secureStore.write(key: _accessTokenKey, value: session.accessToken);
    await _secureStore.write(
      key: _expiresAtKey,
      value: session.expiresAt.toIso8601String(),
    );

    if (session.refreshToken != null) {
      await _secureStore.write(key: _refreshTokenKey, value: session.refreshToken!);
    } else {
      await _secureStore.delete(key: _refreshTokenKey);
    }

    if (session.idToken != null) {
      await _secureStore.write(key: _idTokenKey, value: session.idToken!);
    } else {
      await _secureStore.delete(key: _idTokenKey);
    }

    if (session.tokenType != null) {
      await _secureStore.write(key: _tokenTypeKey, value: session.tokenType!);
    } else {
      await _secureStore.delete(key: _tokenTypeKey);
    }

    return session;
  }

  Future<void> _clearSession() {
    return _secureStore.deleteAll();
  }

  DateTime _defaultExpiry() {
    return DateTime.now().add(const Duration(minutes: 30));
  }

  static bool _hasValue(String? value) {
    return value != null && value.trim().isNotEmpty;
  }

  static String? _normalized(String? value) {
    if (value == null) {
      return null;
    }

    final String trimmed = value.trim();
    return trimmed.isEmpty ? null : trimmed;
  }
}

class AuthRepositoryException implements Exception {
  const AuthRepositoryException(this.message);

  final String message;

  @override
  String toString() => message;
}
