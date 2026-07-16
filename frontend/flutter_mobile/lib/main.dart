import 'package:domu_mobile/app/app.dart';
import 'package:domu_mobile/app/config/auth_configuration.dart';
import 'package:domu_mobile/core/api/api_client.dart';
import 'package:domu_mobile/features/auth/data/flutter_oidc_client.dart';
import 'package:domu_mobile/features/auth/data/secure_session_storage.dart';
import 'package:domu_mobile/features/auth/data/zitadel_auth_repository.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/households/data/api_household_repository.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:domu_mobile/features/members/data/api_members_repository.dart';
import 'package:domu_mobile/features/members/ui/members_view_model.dart';
import 'package:domu_mobile/features/shopping_lists/data/api_shopping_lists_repository.dart';
import 'package:domu_mobile/features/shopping_lists/ui/shopping_list_detail_view_model.dart';
import 'package:domu_mobile/features/shopping_lists/ui/shopping_lists_view_model.dart';
import 'package:domu_mobile/features/spaces/data/api_spaces_repository.dart';
import 'package:domu_mobile/features/spaces/ui/spaces_view_model.dart';
import 'package:flutter/widgets.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  final configuration = AuthConfiguration.fromEnvironment();
  final repository = ZitadelAuthRepository(
    FlutterOidcClient(configuration),
    SecureSessionStorage(),
  );
  final authViewModel = AuthViewModel(repository);
  final householdsViewModel = HouseholdsViewModel(
    ApiHouseholdRepository(
      ApiClient(
        baseUrl: configuration.apiBaseUrl,
        accessToken: authViewModel.validAccessToken,
      ),
    ),
  );
  final membersViewModel = MembersViewModel(
    ApiMembersRepository(
      ApiClient(
        baseUrl: configuration.apiBaseUrl,
        accessToken: authViewModel.validAccessToken,
      ),
    ),
  );
  final shoppingListsRepository = ApiShoppingListsRepository(
    ApiClient(
      baseUrl: configuration.apiBaseUrl,
      accessToken: authViewModel.validAccessToken,
    ),
  );
  final spacesRepository = ApiSpacesRepository(
    ApiClient(
      baseUrl: configuration.apiBaseUrl,
      accessToken: authViewModel.validAccessToken,
    ),
  );
  authViewModel.initialize();
  runApp(
    DomuApp(
      authViewModel: authViewModel,
      householdsViewModel: householdsViewModel,
      membersViewModel: membersViewModel,
      shoppingListsViewModel: ShoppingListsViewModel(shoppingListsRepository),
      shoppingListDetailViewModel: ShoppingListDetailViewModel(
        shoppingListsRepository,
      ),
      spacesViewModel: SpacesViewModel(spacesRepository),
    ),
  );
}
