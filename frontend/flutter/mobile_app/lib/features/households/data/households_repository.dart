import '../../../core/auth/auth_session.dart';
import '../../../core/http/api_client.dart';
import '../domain/household.dart';

abstract class HouseholdsRepository {
  Future<List<Household>> getHouseholds(AuthSession session);
}

class ApiHouseholdsRepository implements HouseholdsRepository {
  const ApiHouseholdsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<Household>> getHouseholds(AuthSession session) async {
    final Object? json = await _apiClient.getJson(
      '/api/v1/households',
      session: session,
    );

    if (json is! List<Object?>) {
      throw const FormatException('Expected a household list.');
    }

    return json
        .whereType<Map<String, Object?>>()
        .map(Household.fromJson)
        .toList(growable: false);
  }
}
