import 'dart:convert';
import '../../../core/api/api_client.dart';
import '../domain/shopping_list.dart';
import '../domain/shopping_lists_repository.dart';
import 'shopping_lists_dto.dart';

class ApiShoppingListsRepository implements ShoppingListsRepository {
  ApiShoppingListsRepository(this._client);
  final ApiClient _client;
  static const _base = '/api/v1/households';
  String _path(String householdId) => '$_base/$householdId/shopping-lists';

  @override
  Future<List<ShoppingList>> getShoppingLists(String householdId) async {
    final response = await _request(() => _client.get(_path(householdId)));
    try {
      final data = _map(_decode(response.body))['data'];
      if (data is! List) throw const FormatException();
      return data
          .map((e) => ShoppingListDto.fromJson(_map(e)))
          .where((e) => e.archivedAt == null)
          .map((e) => e.toDomain())
          .toList(growable: false);
    } catch (_) {
      throw const ShoppingListsRepositoryException(
        'Domu returned an invalid shopping list.',
      );
    }
  }

  @override
  Future<ShoppingList> createShoppingList({
    required String householdId,
    required String name,
  }) => _listRequest(
    () => _client.post(_path(householdId), body: {'name': name}),
  );
  @override
  Future<ShoppingList> renameShoppingList({
    required String householdId,
    required ShoppingList list,
    required String name,
  }) => _listRequest(
    () => _client.put(
      '${_path(householdId)}/${list.id}',
      body: {'name': name, 'archived': false},
    ),
  );
  @override
  Future<void> archiveShoppingList({
    required String householdId,
    required ShoppingList list,
  }) => _voidRequest(() => _client.delete('${_path(householdId)}/${list.id}'));
  @override
  Future<List<ShoppingListItem>> getItems({
    required String householdId,
    required String shoppingListId,
  }) async {
    final response = await _request(
      () => _client.get('${_path(householdId)}/$shoppingListId/items'),
    );
    try {
      final data = _map(_decode(response.body))['data'];
      if (data is! List) throw const FormatException();
      return data
          .map((e) => ShoppingListItemDto.fromJson(_map(e)).toDomain())
          .toList(growable: false);
    } catch (_) {
      throw const ShoppingListsRepositoryException(
        'Domu returned an invalid shopping list item list.',
      );
    }
  }

  @override
  Future<ShoppingListItem> createItem({
    required String householdId,
    required String shoppingListId,
    required String name,
    String? note,
  }) => _itemRequest(
    () => _client.post(
      '${_path(householdId)}/$shoppingListId/items',
      body: {'name': name, 'note': ?note},
    ),
  );
  @override
  Future<ShoppingListItem> updateItem({
    required String householdId,
    required String shoppingListId,
    required ShoppingListItem item,
    required String name,
    String? note,
  }) => _itemRequest(
    () => _client.patch(
      '${_path(householdId)}/$shoppingListId/items/${item.id}',
      body: {
        'name': name,
        'note': note,
        'quantity': item.quantity,
        'containerQuantity': item.containerQuantity,
        'containerUnit': item.containerUnit,
        'spaceId': item.spaceId,
        'itemId': item.itemId,
        'sortOrder': item.sortOrder,
      },
    ),
  );
  @override
  Future<void> setItemChecked({
    required String householdId,
    required String shoppingListId,
    required String itemId,
    required bool checked,
  }) => _voidRequest(
    () => _client.post(
      '${_path(householdId)}/$shoppingListId/items/$itemId/${checked ? 'check' : 'uncheck'}',
    ),
  );
  @override
  Future<void> deleteItem({
    required String householdId,
    required String shoppingListId,
    required String itemId,
  }) => _voidRequest(
    () => _client.delete('${_path(householdId)}/$shoppingListId/items/$itemId'),
  );
  @override
  Future<void> clearCompleted({
    required String householdId,
    required String shoppingListId,
  }) => _voidRequest(
    () => _client.delete('${_path(householdId)}/$shoppingListId/items/checked'),
  );
  Future<ShoppingList> _listRequest(
    Future<ApiResponse> Function() action,
  ) async {
    final response = await _request(action);
    try {
      return ShoppingListDto.fromJson(
        _map(_map(_decode(response.body))['data']),
      ).toDomain();
    } catch (_) {
      throw const ShoppingListsRepositoryException(
        'Domu returned an invalid shopping list response.',
      );
    }
  }

  Future<ShoppingListItem> _itemRequest(
    Future<ApiResponse> Function() action,
  ) async {
    final response = await _request(action);
    try {
      return ShoppingListItemDto.fromJson(
        _map(_map(_decode(response.body))['data']),
      ).toDomain();
    } catch (_) {
      throw const ShoppingListsRepositoryException(
        'Domu returned an invalid shopping list item response.',
      );
    }
  }

  Future<void> _voidRequest(Future<ApiResponse> Function() action) async {
    await _request(action);
  }

  Future<ApiResponse> _request(Future<ApiResponse> Function() action) async {
    try {
      final r = await action();
      if (!r.isSuccess) {
        throw ShoppingListsRepositoryException(_message(r.statusCode));
      }
      return r;
    } on ShoppingListsRepositoryException {
      rethrow;
    } on ApiClientException catch (e) {
      throw ShoppingListsRepositoryException(e.message);
    } catch (_) {
      throw const ShoppingListsRepositoryException(
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
    400 => 'Please check the shopping list details and try again.',
    401 => 'Your session has expired. Please sign in again.',
    403 => 'You do not have permission to do that.',
    404 => 'That shopping list could not be found.',
    _ => 'Unable to complete that request. Please try again.',
  };
}
