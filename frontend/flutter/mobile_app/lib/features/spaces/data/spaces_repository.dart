import '../../../core/auth/auth_session.dart';
import '../../../core/http/api_client.dart';
import '../domain/space.dart';

abstract class SpacesRepository {
  Future<SpacePage> getSpaces({
    required AuthSession session,
    required String householdId,
    String? parentId,
  });

  Future<void> create({
    required AuthSession session,
    required String householdId,
    required String name,
    String? parentId,
    String? description,
  });
}

class ApiSpacesRepository implements SpacesRepository {
  const ApiSpacesRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<SpacePage> getSpaces({
    required AuthSession session,
    required String householdId,
    String? parentId,
  }) async {
    final Object? json = await _apiClient.getJson(
      '/api/v1/households/$householdId/spaces',
      session: session,
      queryParameters: <String, String?>{
        'includeItemCount': 'true',
        'includeChildSpaceCount': 'true',
        'parentId': parentId,
      },
    );

    if (json is! Map<String, Object?>) {
      throw const FormatException('Expected a spaces page.');
    }

    return SpacePage.fromJson(json);
  }

  @override
  Future<void> create({
    required AuthSession session,
    required String householdId,
    required String name,
    String? parentId,
    String? description,
  }) async {
    await _apiClient.postJson(
      '/api/v1/households/$householdId/spaces',
      session: session,
      body: <String, Object?>{
        'name': name,
        'parentId': parentId,
        'description': description,
      },
    );
  }
}
