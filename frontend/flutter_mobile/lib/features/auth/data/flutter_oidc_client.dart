import 'package:domu_mobile/app/config/auth_configuration.dart';
import 'package:flutter_appauth/flutter_appauth.dart';

import '../domain/auth_repository.dart';
import 'oidc_client.dart';

class FlutterOidcClient implements OidcClient {
  FlutterOidcClient(this._configuration, {FlutterAppAuth? appAuth})
    : _appAuth = appAuth ?? const FlutterAppAuth();

  final AuthConfiguration _configuration;
  final FlutterAppAuth _appAuth;

  @override
  Future<OidcTokens> authorize({String? loginHint}) async {
    try {
      final response = await _appAuth.authorizeAndExchangeCode(
        AuthorizationTokenRequest(
          _configuration.clientId,
          _configuration.redirectUri,
          discoveryUrl:
              '${_configuration.issuer}/.well-known/openid-configuration',
          scopes: _configuration.scopes,
          loginHint: loginHint,
          additionalParameters: <String, String>{
            'audience': _configuration.apiAudience,
          },
          allowInsecureConnections: _configuration.https,
        ),
      );
      return OidcTokens(
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        expiresAt: response.accessTokenExpirationDateTime,
      );
    } on FlutterAppAuthUserCancelledException {
      throw const AuthSignInCancelled();
    }
  }

  @override
  Future<OidcTokens> refresh(String refreshToken) async {
    final response = await _appAuth.token(
      TokenRequest(
        _configuration.clientId,
        _configuration.redirectUri,
        discoveryUrl:
            '${_configuration.issuer}/.well-known/openid-configuration',
        refreshToken: refreshToken,
        scopes: _configuration.scopes,
      ),
    );
    return OidcTokens(
      accessToken: response.accessToken,
      // Some providers rotate refresh tokens; keep the old one when omitted.
      refreshToken: response.refreshToken ?? refreshToken,
      expiresAt: response.accessTokenExpirationDateTime,
    );
  }
}
