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
    required this.count,
    required this.amountPerUnit,
    required this.unit,
    required this.spaceId,
    required this.itemId,
    required this.checked,
    required this.sortOrder,
  });

  final String id;
  final String shoppingListId;
  final String name;
  final String? note;
  final int count;
  final num? amountPerUnit;
  final ShoppingListItemUnit? unit;
  final String? spaceId;
  final String? itemId;
  final bool checked;
  final num sortOrder;
}

enum ShoppingListItemUnit {
  unspecified,
  piece,
  milliliter,
  liter,
  gram,
  kilogram,
}
