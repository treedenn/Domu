import 'package:domu_mobile/features/dashboard/ui/dashboard_view_model.dart';
import 'package:domu_mobile/features/households/domain/household.dart';
import 'package:domu_mobile/features/households/domain/household_expiration.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('loads the next 30 days of household expirations', () async {
    final repository = _Repository();
    final viewModel = DashboardViewModel(
      repository,
      now: () => DateTime.utc(2026, 7, 21),
    );

    await viewModel.load('home');

    expect(repository.householdId, 'home');
    expect(repository.upcomingUntil, DateTime.utc(2026, 8, 20));
    expect(viewModel.expirations, isNotNull);
    expect(viewModel.errorMessage, isNull);
  });

  test('surfaces expiration loading failures', () async {
    final viewModel = DashboardViewModel(_Repository(fails: true));

    await viewModel.load('home');

    expect(viewModel.errorMessage, 'Could not load expirations.');
  });
}

class _Repository implements HouseholdRepository {
  _Repository({this.fails = false});

  final bool fails;
  String? householdId;
  DateTime? upcomingUntil;

  @override
  Future<HouseholdExpirations> getHouseholdExpirations({
    required String householdId,
    required DateTime upcomingUntil,
  }) async {
    if (fails) {
      throw const HouseholdRepositoryException('Could not load expirations.');
    }
    this.householdId = householdId;
    this.upcomingUntil = upcomingUntil;
    return HouseholdExpirations(
      evaluatedAt: DateTime.utc(2026, 7, 21),
      expired: const [],
      upcoming: const [],
    );
  }

  @override
  Future<Household> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) => throw UnimplementedError();

  @override
  Future<void> deleteHousehold(String id) => throw UnimplementedError();

  @override
  Future<List<Household>> getHouseholds() => throw UnimplementedError();

  @override
  Future<Household> updateHousehold({
    required String id,
    required String name,
  }) => throw UnimplementedError();
}
