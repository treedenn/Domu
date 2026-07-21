import 'package:domu_mobile/features/households/domain/household.dart';
import 'package:domu_mobile/features/households/domain/household_expiration.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test(
    'loads, selects, and clears selection after deleting a household',
    () async {
      final repository = _FakeRepository();
      final viewModel = HouseholdsViewModel(repository);

      await viewModel.load();
      final household = viewModel.households.single;
      viewModel.selectHousehold(household);
      await viewModel.deleteHousehold(household.id);

      expect(viewModel.households, isEmpty);
      expect(viewModel.selectedHousehold, isNull);
      expect(viewModel.message, 'Household deleted.');
    },
  );

  test('surfaces repository failures', () async {
    final viewModel = HouseholdsViewModel(_FakeRepository(failLoading: true));

    await viewModel.load();

    expect(viewModel.errorMessage, 'Could not load households.');
  });

  test('clears the selection when it disappears during refresh', () async {
    final repository = _FakeRepository();
    final viewModel = HouseholdsViewModel(repository);
    await viewModel.load();
    viewModel.selectHousehold(viewModel.households.single);

    repository.removeAll();
    await viewModel.refresh();

    expect(viewModel.selectedHouseholdId, isNull);
  });
}

class _FakeRepository implements HouseholdRepository {
  _FakeRepository({this.failLoading = false});

  final bool failLoading;
  final List<Household> _households = [
    const Household(
      id: 'household-1',
      name: 'Home',
      subscriptionPlan: HouseholdSubscriptionPlan.free,
      subscriptionStatus: HouseholdSubscriptionStatus.active,
    ),
  ];

  @override
  Future<Household> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) async => throw UnimplementedError();

  @override
  Future<void> deleteHousehold(String id) async {
    _households.removeWhere((household) => household.id == id);
  }

  @override
  Future<HouseholdExpirations> getHouseholdExpirations({
    required String householdId,
    required DateTime upcomingUntil,
  }) async => throw UnimplementedError();

  @override
  Future<List<Household>> getHouseholds() async {
    if (failLoading) {
      throw const HouseholdRepositoryException('Could not load households.');
    }
    return List.of(_households);
  }

  void removeAll() => _households.clear();

  @override
  Future<Household> updateHousehold({
    required String id,
    required String name,
  }) async => throw UnimplementedError();
}
