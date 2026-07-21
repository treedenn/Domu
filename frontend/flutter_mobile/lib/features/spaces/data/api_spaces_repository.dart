import 'dart:convert';

import '../../../core/api/api_client.dart';
import '../domain/space.dart';
import '../domain/spaces_repository.dart';
import 'spaces_dto.dart';

class ApiSpacesRepository implements SpacesRepository {
  ApiSpacesRepository(this._client);
  final ApiClient _client;
  static const _base = '/api/v1/households';
  String _spaces(String householdId) => '$_base/$householdId/spaces';
  String _items(String householdId, String spaceId) =>
      '${_spaces(householdId)}/$spaceId/items';

  @override
  Future<SpacePage> getSpaces({
    required String householdId,
    String? parentId,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    final query = <String, String>{
      'pageNumber': '$pageNumber',
      'pageSize': '$pageSize',
      'includeItemCount': 'true',
      'includeChildSpaceCount': 'true',
      if (parentId != null) 'parentId': parentId,
    };
    final response = await _request(
      () => _client.get(
        Uri(path: _spaces(householdId), queryParameters: query).toString(),
      ),
    );
    try {
      final data = _map(_map(_decode(response.body))['data']);
      final spaces = (data['spaces'] as List)
          .map((value) => SpaceDto(_map(value)).toDomain())
          .toList(growable: false);
      return SpacePage(
        spaces: spaces,
        pageNumber: data['pageNumber'] as int,
        pageSize: data['pageSize'] as int,
        totalCount: data['totalCount'] as int,
      );
    } catch (_) {
      throw const SpacesRepositoryException(
        'Domu returned an invalid spaces response.',
      );
    }
  }

  @override
  Future<Space> getSpace({
    required String householdId,
    required String spaceId,
  }) => _spaceRequest(() => _client.get('${_spaces(householdId)}/$spaceId'));
  @override
  Future<Space> createSpace({
    required String householdId,
    required String name,
    String? description,
    String? parentId,
  }) => _spaceRequest(
    () => _client.post(
      _spaces(householdId),
      body: {'name': name, 'description': description, 'parentId': parentId},
    ),
  );
  @override
  Future<Space> updateSpace({
    required String householdId,
    required String spaceId,
    required String name,
    String? description,
  }) => _spaceRequest(
    () => _client.put(
      '${_spaces(householdId)}/$spaceId',
      body: {'name': name, 'description': description},
    ),
  );
  @override
  Future<Space> moveSpace({
    required String householdId,
    required String spaceId,
    String? parentId,
  }) => _spaceRequest(
    () => _client.put(
      '${_spaces(householdId)}/$spaceId/parent',
      body: {'parentId': parentId},
    ),
  );
  @override
  Future<void> deleteSpace({
    required String householdId,
    required String spaceId,
  }) => _voidRequest(() => _client.delete('${_spaces(householdId)}/$spaceId'));
  @override
  Future<List<SpaceItem>> getItems({
    required String householdId,
    required String spaceId,
  }) async {
    final response = await _request(
      () => _client.get(_items(householdId, spaceId)),
    );
    try {
      return (_map(_decode(response.body))['data'] as List)
          .map((value) => SpaceItemDto(_map(value)).toDomain())
          .toList(growable: false);
    } catch (_) {
      throw const SpacesRepositoryException(
        'Domu returned an invalid item list.',
      );
    }
  }

  @override
  Future<SpaceItem> createItem({
    required String householdId,
    required String spaceId,
    required String name,
    String? category,
    String? barcode,
    List<ItemEntry>? entries,
  }) => _itemRequest(
    () => _client.post(
      _items(householdId, spaceId),
      body: {
        'name': name,
        'category': category,
        'barcode': barcode,
        if (entries != null) 'entries': entries.map(_entry).toList(),
      },
    ),
  );
  @override
  Future<SpaceItem> updateItem({
    required String householdId,
    required String spaceId,
    required String itemId,
    required String name,
    String? category,
    String? barcode,
  }) => _itemRequest(
    () => _client.put(
      '${_items(householdId, spaceId)}/$itemId',
      body: {'name': name, 'category': category, 'barcode': barcode},
    ),
  );
  @override
  Future<SpaceItem> replaceItemEntries({
    required String householdId,
    required String spaceId,
    required String itemId,
    required List<ItemEntry> entries,
  }) => _itemRequest(
    () => _client.put(
      '${_items(householdId, spaceId)}/$itemId/entries',
      body: {'entries': entries.map(_entry).toList()},
    ),
  );
  @override
  Future<void> deleteItem({
    required String householdId,
    required String spaceId,
    required String itemId,
  }) => _voidRequest(
    () => _client.delete('${_items(householdId, spaceId)}/$itemId'),
  );

  Map<String, Object?> _entry(ItemEntry entry) => {
    'id': entry.id,
    'count': entry.count,
    'originalAmountPerUnit': entry.originalAmountPerUnit,
    'currentAmountPerUnit': entry.state == ConsumableState.unopened
        ? entry.originalAmountPerUnit
        : entry.currentAmountPerUnit,
    'unit': entry.unit.name,
    'state': entry.state.name,
    'acquisitionDate': entry.acquisitionDate?.toIso8601String(),
    'expirationDate': entry.expirationDate?.toIso8601String(),
  };
  Future<Space> _spaceRequest(Future<ApiResponse> Function() action) async {
    final response = await _request(action);
    try {
      return SpaceDto(_map(_map(_decode(response.body))['data'])).toDomain();
    } catch (_) {
      throw const SpacesRepositoryException(
        'Domu returned an invalid space response.',
      );
    }
  }

  Future<SpaceItem> _itemRequest(Future<ApiResponse> Function() action) async {
    final response = await _request(action);
    try {
      return SpaceItemDto(
        _map(_map(_decode(response.body))['data']),
      ).toDomain();
    } catch (_) {
      throw const SpacesRepositoryException(
        'Domu returned an invalid item response.',
      );
    }
  }

  Future<void> _voidRequest(Future<ApiResponse> Function() action) async {
    await _request(action);
  }

  Future<ApiResponse> _request(Future<ApiResponse> Function() action) async {
    try {
      final response = await action();
      if (!response.isSuccess)
        throw SpacesRepositoryException(_message(response.statusCode));
      return response;
    } on SpacesRepositoryException {
      rethrow;
    } on ApiClientException catch (e) {
      throw SpacesRepositoryException(e.message);
    } catch (_) {
      throw const SpacesRepositoryException(
        'Unable to complete that request. Please try again.',
      );
    }
  }

  static dynamic _decode(String body) => jsonDecode(body);
  static Map<String, dynamic> _map(Object? value) {
    if (value is Map<String, dynamic>) return value;
    if (value is Map) return Map<String, dynamic>.from(value);
    throw const FormatException();
  }

  static String _message(int status) => switch (status) {
    400 => 'Please check the space details and try again.',
    401 => 'Your session has expired. Please sign in again.',
    403 => 'You do not have permission to do that.',
    404 => 'That space could not be found.',
    409 => 'Delete child spaces and items before deleting this space.',
    _ => 'Unable to complete that request. Please try again.',
  };
}
