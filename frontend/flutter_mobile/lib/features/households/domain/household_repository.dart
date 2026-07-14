import 'household.dart';

abstract interface class HouseholdRepository {
  Future<List<Household>> getHouseholds();
  Future<Household> createHousehold({
    required String name,
    required String ownerDisplayName,
  });
  Future<Household> updateHousehold({required String id, required String name});
  Future<void> deleteHousehold(String id);
}

class HouseholdRepositoryException implements Exception {
  const HouseholdRepositoryException(this.message);

  final String message;

  @override
  String toString() => message;
}
