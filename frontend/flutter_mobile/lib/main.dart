import 'package:domu_mobile/app/app.dart';
import 'package:domu_mobile/app/config/auth_configuration.dart';
import 'package:domu_mobile/features/auth/data/flutter_oidc_client.dart';
import 'package:domu_mobile/features/auth/data/secure_session_storage.dart';
import 'package:domu_mobile/features/auth/data/zitadel_auth_repository.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:flutter/widgets.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  final configuration = AuthConfiguration.fromEnvironment();
  final repository = ZitadelAuthRepository(
    FlutterOidcClient(configuration),
    SecureSessionStorage(),
  );
  final authViewModel = AuthViewModel(repository);
  authViewModel.initialize();
  runApp(DomuApp(authViewModel: authViewModel));
}
