import 'space.dart';

abstract interface class SpacesRepository {
  Future<SpacePage> getSpaces({
    required String householdId,
    String? parentId,
    int pageNumber = 1,
    int pageSize = 20,
  });
  Future<Space> getSpace({
    required String householdId,
    required String spaceId,
  });
  Future<Space> createSpace({
    required String householdId,
    required String name,
    String? description,
    String? parentId,
  });
  Future<Space> updateSpace({
    required String householdId,
    required String spaceId,
    required String name,
    String? description,
  });
  Future<Space> moveSpace({
    required String householdId,
    required String spaceId,
    String? parentId,
  });
  Future<void> deleteSpace({
    required String householdId,
    required String spaceId,
  });
  Future<List<SpaceItem>> getItems({
    required String householdId,
    required String spaceId,
  });
  Future<SpaceItem> createItem({
    required String householdId,
    required String spaceId,
    required String name,
    String? category,
    String? barcode,
    List<ItemEntry>? entries,
  });
  Future<SpaceItem> updateItem({
    required String householdId,
    required String spaceId,
    required String itemId,
    required String name,
    String? category,
    String? barcode,
  });
  Future<SpaceItem> replaceItemEntries({
    required String householdId,
    required String spaceId,
    required String itemId,
    required List<ItemEntry> entries,
  });
  Future<void> deleteItem({
    required String householdId,
    required String spaceId,
    required String itemId,
  });
}

class SpacesRepositoryException implements Exception {
  const SpacesRepositoryException(this.message);
  final String message;
  @override
  String toString() => message;
}
