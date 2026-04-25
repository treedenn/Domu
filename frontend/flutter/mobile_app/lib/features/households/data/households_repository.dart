import '../domain/household.dart';

abstract class HouseholdsRepository {
  Future<List<Household>> getHouseholds();
}

class StubHouseholdsRepository implements HouseholdsRepository {
  @override
  Future<List<Household>> getHouseholds() async {
    return const <Household>[];
  }
}
