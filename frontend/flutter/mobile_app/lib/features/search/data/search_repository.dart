import '../../../core/auth/auth_session.dart';
import '../../../core/http/api_client.dart';
import '../../items/domain/item.dart';
import '../../spaces/domain/space.dart';
import '../domain/search_engine.dart';
import '../domain/search_query.dart';

abstract class SearchRepository {
  Future<SearchResults> search({
    required AuthSession session,
    required String householdId,
    required SearchQuery query,
  });
}

class ApiSearchRepository implements SearchRepository {
  const ApiSearchRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<SearchResults> search({
    required AuthSession session,
    required String householdId,
    required SearchQuery query,
  }) async {
    final Object? json = await _apiClient.getJson(
      '/api/v1/households/$householdId/search',
      session: session,
      queryParameters: <String, String?>{
        'text': query.text,
        'expiringWithinDays': query.expiringWithinDays?.toString(),
      },
    );

    if (json is! Map<String, Object?>) {
      throw const FormatException('Expected search results.');
    }

    final Object? spacesJson = json['spaces'];
    final Object? itemsJson = json['items'];

    return SearchResults(
      spaces: spacesJson is List<Object?>
          ? spacesJson
                .whereType<Map<String, Object?>>()
                .map(Space.fromJson)
                .toList(growable: false)
          : const <Space>[],
      items: itemsJson is List<Object?>
          ? itemsJson
                .whereType<Map<String, Object?>>()
                .map(Item.fromJson)
                .toList(growable: false)
          : const <Item>[],
    );
  }
}
