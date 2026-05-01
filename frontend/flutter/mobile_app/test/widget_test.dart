import 'package:flutter_test/flutter_test.dart';

import 'package:domu_mobile_app/app/app.dart';
import 'package:domu_mobile_app/app/bootstrap/app_bootstrap.dart';
import 'package:domu_mobile_app/app/bootstrap/app_config.dart';
import 'package:domu_mobile_app/features/auth/data/auth_repository.dart';
import 'package:domu_mobile_app/features/auth/presentation/controllers/auth_controller.dart';
import 'package:domu_mobile_app/core/auth/auth_session.dart';
import 'package:domu_mobile_app/features/households/data/households_repository.dart';
import 'package:domu_mobile_app/features/households/data/members_repository.dart';
import 'package:domu_mobile_app/features/households/domain/household.dart';
import 'package:domu_mobile_app/features/households/domain/member.dart';
import 'package:domu_mobile_app/features/items/data/items_repository.dart';
import 'package:domu_mobile_app/features/items/domain/item.dart';
import 'package:domu_mobile_app/features/items/domain/item_entry.dart';
import 'package:domu_mobile_app/features/spaces/data/spaces_repository.dart';
import 'package:domu_mobile_app/features/spaces/domain/space.dart';

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

class FakeHouseholdsRepository implements HouseholdsRepository {
  @override
  Future<List<Household>> getHouseholds(AuthSession session) async {
    return const <Household>[];
  }
}

class FakeMembersRepository implements MembersRepository {
  @override
  Future<List<Member>> getMembers({
    required AuthSession session,
    required String householdId,
  }) async {
    return const <Member>[];
  }

  @override
  Future<void> invite({
    required AuthSession session,
    required String householdId,
    required String email,
    required MemberRole role,
  }) async {}
}

class FakeSpacesRepository implements SpacesRepository {
  @override
  Future<void> create({
    required AuthSession session,
    required String householdId,
    required String name,
    String? parentId,
    String? description,
  }) async {}

  @override
  Future<SpacePage> getSpaces({
    required AuthSession session,
    required String householdId,
    String? parentId,
  }) async {
    return const SpacePage(
      spaces: <Space>[],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 0,
    );
  }
}

class FakeItemsRepository implements ItemsRepository {
  @override
  Future<Item> addItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String name,
    String? barcode,
  }) {
    throw UnimplementedError();
  }

  @override
  Future<void> deleteEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
    required String entryId,
  }) async {}

  @override
  Future<List<ItemEntry>> getEntries({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  }) async {
    return const <ItemEntry>[];
  }

  @override
  Future<Item?> getItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  }) async {
    return null;
  }

  @override
  Future<List<Item>> getItems({
    required AuthSession session,
    required String householdId,
    required String spaceId,
  }) async {
    return const <Item>[];
  }

  @override
  Future<ItemEntry> saveEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required ItemEntry entry,
  }) async {
    return entry;
  }

  @override
  Future<List<Item>> searchItems({
    required AuthSession session,
    required String householdId,
  }) async {
    return const <Item>[];
  }
}

void main() {
  testWidgets('renders bootstrap shell', (WidgetTester tester) async {
    final AuthController authController = AuthController(FakeAuthRepository());
    await authController.restoreSession();
    final AppConfig config = const AppConfig(
      appName: 'Domu',
      apiBaseUrl: 'http://localhost:8080',
      oidcIssuer: 'http://localhost:8081',
      oidcClientId: 'domu-mobile',
      oidcRedirectUri: 'domu://auth/callback',
      googleIdpId: 'google-idp',
      facebookIdpId: 'facebook-idp',
    );

    await tester.pumpWidget(
      DomuApp(
        bootstrap: AppBootstrap(
          config: config,
          authController: authController,
          householdsRepository: FakeHouseholdsRepository(),
          membersRepository: FakeMembersRepository(),
          spacesRepository: FakeSpacesRepository(),
          itemsRepository: FakeItemsRepository(),
        ),
      ),
    );

    expect(find.text('Sign in to Domu'), findsOneWidget);
    expect(find.text('Continue with Google'), findsOneWidget);
  });
}
