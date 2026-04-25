class AppConfig {
  const AppConfig({
    required this.appName,
    required this.apiBaseUrl,
    required this.oidcIssuer,
    required this.oidcClientId,
    required this.oidcRedirectUri,
    required this.googleIdpId,
    required this.facebookIdpId,
  });

  const AppConfig.fromEnvironment()
      : appName = const String.fromEnvironment(
          'APP_NAME',
          defaultValue: 'Domu',
        ),
        apiBaseUrl = const String.fromEnvironment(
          'API_BASE_URL',
          defaultValue: 'http://localhost:8080',
        ),
        oidcIssuer = const String.fromEnvironment(
          'OIDC_ISSUER',
          defaultValue: 'http://localhost:8081',
        ),
        oidcClientId = const String.fromEnvironment(
          'OIDC_CLIENT_ID',
          defaultValue: 'domu-mobile',
        ),
        oidcRedirectUri = const String.fromEnvironment(
          'OIDC_REDIRECT_URI',
          defaultValue: 'domu://auth/callback',
        ),
        googleIdpId = const String.fromEnvironment(
          'OIDC_GOOGLE_IDP_ID',
          defaultValue: '',
        ),
        facebookIdpId = const String.fromEnvironment(
          'OIDC_FACEBOOK_IDP_ID',
          defaultValue: '',
        );

  final String appName;
  final String apiBaseUrl;
  final String oidcIssuer;
  final String oidcClientId;
  final String oidcRedirectUri;
  final String googleIdpId;
  final String facebookIdpId;

  bool get allowInsecureConnections => oidcIssuer.startsWith('http://');

  List<String> get authScopes => const <String>[
        'openid',
        'profile',
        'email',
        'offline_access',
      ];

  bool get hasGoogleSso => _hasValue(googleIdpId);

  bool get hasFacebookSso => _hasValue(facebookIdpId);

  static bool _hasValue(String value) {
    return value.trim().isNotEmpty;
  }
}
