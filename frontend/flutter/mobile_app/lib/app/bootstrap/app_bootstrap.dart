import 'package:flutter_appauth/flutter_appauth.dart';

import '../../core/http/api_client.dart';
import '../../core/storage/secure_store.dart';
import '../../features/auth/data/auth_repository.dart';
import '../../features/auth/data/zitadel_auth_repository.dart';
import '../../features/auth/presentation/controllers/auth_controller.dart';
import '../../features/households/data/households_repository.dart';
import '../../features/households/data/members_repository.dart';
import '../../features/items/data/items_repository.dart';
import '../../features/search/data/search_repository.dart';
import '../../features/spaces/data/spaces_repository.dart';
import 'app_config.dart';

class AppBootstrap {
  AppBootstrap({
    required this.config,
    required this.authController,
    required this.householdsRepository,
    required this.membersRepository,
    required this.spacesRepository,
    required this.itemsRepository,
    required this.searchRepository,
  });

  final AppConfig config;
  final AuthController authController;
  final HouseholdsRepository householdsRepository;
  final MembersRepository membersRepository;
  final SpacesRepository spacesRepository;
  final ItemsRepository itemsRepository;
  final SearchRepository searchRepository;

  static Future<AppBootstrap> initialize() async {
    final AppConfig config = const AppConfig.fromEnvironment();
    final SecureStore secureStore = FlutterSecureStoreAdapter();
    final AuthRepository authRepository = ZitadelAuthRepository(
      appAuth: FlutterAppAuth(),
      config: config,
      secureStore: secureStore,
    );
    final AuthController authController = AuthController(authRepository);
    final ApiClient apiClient = ApiClient(baseUrl: config.apiBaseUrl);

    await authController.restoreSession();

    return AppBootstrap(
      config: config,
      authController: authController,
      householdsRepository: ApiHouseholdsRepository(apiClient),
      membersRepository: ApiMembersRepository(apiClient),
      spacesRepository: ApiSpacesRepository(apiClient),
      itemsRepository: ApiItemsRepository(apiClient),
      searchRepository: ApiSearchRepository(apiClient),
    );
  }
}
