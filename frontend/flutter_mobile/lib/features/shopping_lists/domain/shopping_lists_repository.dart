import 'shopping_list.dart';

abstract interface class ShoppingListsRepository {
  Future<List<ShoppingList>> getShoppingLists(String householdId);
  Future<ShoppingList> createShoppingList({
    required String householdId,
    required String name,
  });
  Future<ShoppingList> renameShoppingList({
    required String householdId,
    required ShoppingList list,
    required String name,
  });
  Future<void> archiveShoppingList({
    required String householdId,
    required ShoppingList list,
  });
  Future<List<ShoppingListItem>> getItems({
    required String householdId,
    required String shoppingListId,
  });
  Future<ShoppingListItem> createItem({
    required String householdId,
    required String shoppingListId,
    required String name,
    String? note,
  });
  Future<ShoppingListItem> updateItem({
    required String householdId,
    required String shoppingListId,
    required ShoppingListItem item,
    required String name,
    String? note,
  });
  Future<void> setItemChecked({
    required String householdId,
    required String shoppingListId,
    required String itemId,
    required bool checked,
  });
  Future<void> deleteItem({
    required String householdId,
    required String shoppingListId,
    required String itemId,
  });
  Future<void> clearCompleted({
    required String householdId,
    required String shoppingListId,
  });
}

class ShoppingListsRepositoryException implements Exception {
  const ShoppingListsRepositoryException(this.message);
  final String message;
  @override
  String toString() => message;
}
