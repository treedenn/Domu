/// Compile-time configuration for the mobile OIDC client.
///
/// Values are supplied with `--dart-define`; none are safe to hard-code in a
/// mobile build because local URLs differ between simulators and real devices.
class AuthConfiguration {
  AuthConfiguration({
    required this.issuer,
    required this.clientId,
    required this.redirectUri,
    required this.https,
    required this.apiAudience,
    required this.apiScope,
    required this.apiBaseUrl,
  });

  factory AuthConfiguration.fromEnvironment() {
    const issuer = String.fromEnvironment('DOMU_OIDC_ISSUER');
    const clientId = String.fromEnvironment('DOMU_OIDC_CLIENT_ID');
    const redirectUri = String.fromEnvironment('DOMU_OIDC_REDIRECT_URI');
    const https = bool.fromEnvironment(
      'DOMU_OIDC_ALLOW_HTTPS_ONLY',
      defaultValue: true,
    );
    const apiAudience = String.fromEnvironment('DOMU_API_AUDIENCE');
    const apiScope = String.fromEnvironment('DOMU_API_SCOPE');
    const apiBaseUrl = String.fromEnvironment('DOMU_API_BASE_URL');
    final values = <String, String>{
      'DOMU_OIDC_ISSUER': issuer,
      'DOMU_OIDC_CLIENT_ID': clientId,
      'DOMU_OIDC_REDIRECT_URI': redirectUri,
      'DOMU_API_AUDIENCE': apiAudience,
      'DOMU_API_BASE_URL': apiBaseUrl,
    };
    final missing = values.entries
        .where((entry) => entry.value.trim().isEmpty)
        .map((entry) => entry.key)
        .join(', ');
    if (missing.isNotEmpty) {
      throw StateError('Missing required dart-defines: $missing');
    }
    return AuthConfiguration(
      issuer: issuer,
      clientId: clientId,
      redirectUri: redirectUri,
      https: https,
      apiAudience: apiAudience,
      apiScope: apiScope,
      apiBaseUrl: apiBaseUrl,
    );
  }

  final String issuer;
  final String clientId;
  final String redirectUri;
  final bool https;
  final String apiAudience;
  final String apiScope;
  final String apiBaseUrl;

  List<String> get scopes => <String>[
    'openid',
    'profile',
    'email',
    'offline_access',
    if (apiScope.trim().isNotEmpty) apiScope.trim(),
  ];
}
