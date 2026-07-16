class ShoppingList {
  const ShoppingList({
    required this.id,
    required this.householdId,
    required this.name,
  });

  final String id;
  final String householdId;
  final String name;
}

class ShoppingListItem {
  const ShoppingListItem({
    required this.id,
    required this.shoppingListId,
    required this.name,
    required this.note,
    required this.quantity,
    required this.containerQuantity,
    required this.containerUnit,
    required this.spaceId,
    required this.itemId,
    required this.checked,
    required this.sortOrder,
  });

  final String id;
  final String shoppingListId;
  final String name;
  final String? note;
  final num? quantity;
  final num? containerQuantity;
  final String? containerUnit;
  final String? spaceId;
  final String? itemId;
  final bool checked;
  final int sortOrder;
}
