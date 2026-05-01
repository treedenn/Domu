import '../../../core/auth/auth_session.dart';
import '../../../core/http/api_client.dart';
import '../domain/item.dart';
import '../domain/item_entry.dart';

abstract class ItemsRepository {
  Future<List<Item>> getItems({
    required AuthSession session,
    required String householdId,
    required String spaceId,
  });

  Future<Item?> getItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  });

  Future<List<ItemEntry>> getEntries({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  });

  Future<Item> addItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String name,
    String? barcode,
  });

  Future<ItemEntry> saveEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required ItemEntry entry,
  });

  Future<void> deleteEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
    required String entryId,
  });

  Future<List<Item>> searchItems({
    required AuthSession session,
    required String householdId,
  });
}

class ApiItemsRepository implements ItemsRepository {
  const ApiItemsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<Item> addItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String name,
    String? barcode,
  }) async {
    final Object? json = await _apiClient.postJson(
      _itemsPath(householdId, spaceId),
      session: session,
      body: <String, Object?>{
        'name': name,
        'category': null,
        'barcode': barcode == null || barcode.isEmpty ? null : barcode,
        'entries': const <Object?>[],
      },
    );

    if (json is! Map<String, Object?>) {
      throw const FormatException('Expected an item.');
    }

    return Item.fromJson(json);
  }

  @override
  Future<void> deleteEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
    required String entryId,
  }) async {
    final Item? item = await getItem(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: itemId,
    );
    if (item == null) {
      return;
    }

    final List<ItemEntry> entries = await getEntries(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: itemId,
    );
    await _replaceEntries(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: itemId,
      entries: entries
          .where((ItemEntry entry) => entry.id != entryId)
          .toList(growable: false),
    );
  }

  @override
  Future<List<ItemEntry>> getEntries({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  }) async {
    final Item? item = await getItem(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: itemId,
    );
    if (item == null) {
      return const <ItemEntry>[];
    }

    final Object? json = await _apiClient.getJson(
      _itemsPath(householdId, spaceId),
      session: session,
    );
    if (json is! List<Object?>) {
      throw const FormatException('Expected an item list.');
    }
    final Map<String, Object?>? itemJson = json
        .whereType<Map<String, Object?>>()
        .where((Map<String, Object?> value) => value['id'].toString() == itemId)
        .firstOrNull;
    final Object? entriesJson = itemJson?['entries'];
    return entriesJson is List<Object?>
        ? entriesJson
            .whereType<Map<String, Object?>>()
            .map((Map<String, Object?> entryJson) => ItemEntry.fromJson(
                  itemId: itemId,
                  json: entryJson,
                ))
            .toList(growable: false)
        : const <ItemEntry>[];
  }

  @override
  Future<Item?> getItem({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
  }) async {
    final List<Item> items = await getItems(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
    );
    return items.where((Item item) => item.id == itemId).firstOrNull;
  }

  @override
  Future<List<Item>> getItems({
    required AuthSession session,
    required String householdId,
    required String spaceId,
  }) async {
    final Object? json = await _apiClient.getJson(
      _itemsPath(householdId, spaceId),
      session: session,
    );

    if (json is! List<Object?>) {
      throw const FormatException('Expected an item list.');
    }

    return json
        .whereType<Map<String, Object?>>()
        .map(Item.fromJson)
        .toList(growable: false);
  }

  @override
  Future<ItemEntry> saveEntry({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required ItemEntry entry,
  }) async {
    final List<ItemEntry> entries = await getEntries(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: entry.itemId,
    );
    final int index =
        entries.indexWhere((ItemEntry value) => value.id == entry.id);
    final List<ItemEntry> updated = List<ItemEntry>.of(entries);
    if (index == -1) {
      updated.add(entry);
    } else {
      updated[index] = entry;
    }

    final Item item = await _replaceEntries(
      session: session,
      householdId: householdId,
      spaceId: spaceId,
      itemId: entry.itemId,
      entries: updated,
    );
    return item.entries
        .where((ItemEntry saved) =>
            entry.id.isNotEmpty ? saved.id == entry.id : saved.quantity == entry.quantity)
        .lastOrNull ??
        entry;
  }

  @override
  Future<List<Item>> searchItems({
    required AuthSession session,
    required String householdId,
  }) async {
    return const <Item>[];
  }

  Future<Item> _replaceEntries({
    required AuthSession session,
    required String householdId,
    required String spaceId,
    required String itemId,
    required List<ItemEntry> entries,
  }) async {
    final Object? json = await _apiClient.putJson(
      '${_itemsPath(householdId, spaceId)}/$itemId/entries',
      session: session,
      body: <String, Object?>{
        'entries': entries.map((ItemEntry entry) => entry.toJson()).toList(),
      },
    );

    if (json is! Map<String, Object?>) {
      throw const FormatException('Expected an item.');
    }

    return Item.fromJson(json);
  }

  String _itemsPath(String householdId, String spaceId) {
    return '/api/v1/households/$householdId/spaces/$spaceId/items';
  }
}
