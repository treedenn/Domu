import 'app_config.dart';
import '../../core/storage/secure_store.dart';
import '../../features/auth/data/auth_repository.dart';
import '../../features/auth/data/zitadel_auth_repository.dart';
import '../../features/auth/presentation/controllers/auth_controller.dart';
import 'package:flutter_appauth/flutter_appauth.dart';

class AppBootstrap {
  AppBootstrap({
    required this.config,
    required this.authController,
  });

  final AppConfig config;
  final AuthController authController;

  static Future<AppBootstrap> initialize() async {
    final AppConfig config = const AppConfig.fromEnvironment();
    final SecureStore secureStore = FlutterSecureStoreAdapter();
    final AuthRepository authRepository = ZitadelAuthRepository(
      appAuth: FlutterAppAuth(),
      config: config,
      secureStore: secureStore,
    );
    final AuthController authController = AuthController(authRepository);

    await authController.restoreSession();

    return AppBootstrap(
      config: config,
      authController: authController,
    );
  }
}
