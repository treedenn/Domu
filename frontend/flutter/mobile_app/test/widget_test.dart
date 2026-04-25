import 'package:flutter_test/flutter_test.dart';

import 'package:domu_mobile_app/app/app.dart';
import 'package:domu_mobile_app/app/bootstrap/app_bootstrap.dart';
import 'package:domu_mobile_app/app/bootstrap/app_config.dart';
import 'package:domu_mobile_app/features/auth/data/auth_repository.dart';
import 'package:domu_mobile_app/features/auth/presentation/controllers/auth_controller.dart';

class FakeAuthRepository implements AuthRepository {
  @override
  Future<Never> signIn({
    String? loginHint,
    String? preferredIdpId,
    bool createAccount = false,
  }) {
    throw UnimplementedError();
  }

  @override
  Future<void> signOut(session) async {}

  @override
  Future<Null> restoreSession() async {
    return null;
  }
}

void main() {
  testWidgets('renders bootstrap shell', (WidgetTester tester) async {
    final AuthController authController = AuthController(FakeAuthRepository());
    await authController.restoreSession();

    await tester.pumpWidget(
      DomuApp(
        bootstrap: AppBootstrap(
          config: const AppConfig(
            appName: 'Domu',
            apiBaseUrl: 'http://localhost:8080',
            oidcIssuer: 'http://localhost:8081',
            oidcClientId: 'domu-mobile',
            oidcRedirectUri: 'domu://auth/callback',
            googleIdpId: 'google-idp',
            facebookIdpId: 'facebook-idp',
          ),
          authController: authController,
        ),
      ),
    );

    expect(find.text('Sign in to Domu'), findsOneWidget);
    expect(find.text('Continue with Google'), findsOneWidget);
  });
}
