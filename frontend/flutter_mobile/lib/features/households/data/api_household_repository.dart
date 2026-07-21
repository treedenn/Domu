import 'dart:convert';

import '../../../core/api/api_client.dart';
import '../domain/household.dart';
import '../domain/household_expiration.dart';
import '../domain/household_repository.dart';
import 'household_dto.dart';
import 'household_expiration_dto.dart';

class ApiHouseholdRepository implements HouseholdRepository {
  ApiHouseholdRepository(this._client);

  final ApiClient _client;
  static const _path = '/api/v1/households';

  @override
  Future<List<Household>> getHouseholds() async {
    final response = await _request(() => _client.get(_path));
    final json = _decode(response.body);
    if (json is! Map) {
      throw const HouseholdRepositoryException(
        'Domu returned an invalid household list.',
      );
    }
    try {
      final data = _asMap(json)['data'];
      if (data is! List) {
        throw const FormatException('Invalid household list.');
      }
      return data
          .map((item) => HouseholdDto.fromJson(_asMap(item)).toDomain())
          .toList(growable: false);
    } on FormatException {
      throw const HouseholdRepositoryException(
        'Domu returned an invalid household list.',
      );
    }
  }

  @override
  Future<HouseholdExpirations> getHouseholdExpirations({
    required String householdId,
    required DateTime upcomingUntil,
  }) async {
    final uri = Uri(
      path: '$_path/$householdId/expirations',
      queryParameters: {
        'upcomingUntilUtc': upcomingUntil.toUtc().toIso8601String(),
      },
    );
    final response = await _request(() => _client.get(uri.toString()));
    try {
      final data = _asMap(_asMap(_decode(response.body))['data']);
      List<HouseholdExpiration> expirations(String name) => (data[name] as List)
          .map(
            (value) =>
                HouseholdExpirationDto.fromJson(_asMap(value)).toDomain(),
          )
          .toList(growable: false);
      return HouseholdExpirations(
        evaluatedAt: DateTime.parse(data['evaluatedAtUtc'] as String),
        expired: expirations('expired'),
        upcoming: expirations('upcoming'),
      );
    } on FormatException {
      throw const HouseholdRepositoryException(
        'Domu returned an invalid expiration response.',
      );
    } catch (_) {
      throw const HouseholdRepositoryException(
        'Domu returned an invalid expiration response.',
      );
    }
  }

  @override
  Future<Household> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) => _householdRequest(
    () => _client.post(
      _path,
      body: {'name': name, 'ownerDisplayName': ownerDisplayName},
    ),
  );

  @override
  Future<Household> updateHousehold({
    required String id,
    required String name,
  }) =>
      _householdRequest(() => _client.put('$_path/$id', body: {'name': name}));

  @override
  Future<void> deleteHousehold(String id) async {
    await _request(() => _client.delete('$_path/$id'));
  }

  Future<Household> _householdRequest(
    Future<ApiResponse> Function() action,
  ) async {
    final response = await _request(action);
    try {
      final envelope = _asMap(_decode(response.body));
      return HouseholdDto.fromJson(_asMap(envelope['data'])).toDomain();
    } on FormatException {
      throw const HouseholdRepositoryException(
        'Domu returned an invalid household response.',
      );
    }
  }

  Future<ApiResponse> _request(Future<ApiResponse> Function() action) async {
    try {
      final response = await action();
      if (!response.isSuccess) {
        throw HouseholdRepositoryException(_messageFor(response));
      }
      return response;
    } on HouseholdRepositoryException {
      rethrow;
    } on ApiClientException catch (error) {
      throw HouseholdRepositoryException(error.message);
    } catch (_) {
      throw const HouseholdRepositoryException(
        'Unable to complete that request. Please try again.',
      );
    }
  }

  static dynamic _decode(String body) {
    try {
      return jsonDecode(body);
    } on FormatException {
      throw const FormatException('Invalid JSON.');
    }
  }

  static Map<String, dynamic> _asMap(Object? value) {
    if (value is Map<String, dynamic>) {
      return value;
    }
    if (value is Map) {
      return Map<String, dynamic>.from(value);
    }
    throw const FormatException('Expected an object.');
  }

  static String _messageFor(ApiResponse response) {
    if (response.statusCode == 401) {
      return 'Your session has expired. Please sign in again.';
    }
    if (response.statusCode == 403) {
      return 'You do not have permission to do that.';
    }
    if (response.statusCode == 404) {
      return 'That household could not be found.';
    }
    if (response.statusCode == 400) {
      return 'Please check the household details and try again.';
    }
    return 'Unable to complete that request. Please try again.';
  }
}
