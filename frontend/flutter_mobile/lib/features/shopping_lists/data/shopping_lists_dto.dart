import '../domain/shopping_list.dart';

class ShoppingListDto {
  const ShoppingListDto({
    required this.id,
    required this.householdId,
    required this.name,
    required this.archivedAt,
  });
  final String id;
  final String householdId;
  final String name;
  final Object? archivedAt;
  factory ShoppingListDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final householdId = json['householdId'];
    final name = json['name'];
    if (id is! String || householdId is! String || name is! String) {
      throw const FormatException();
    }
    return ShoppingListDto(
      id: id,
      householdId: householdId,
      name: name,
      archivedAt: json['archivedAt'],
    );
  }
  ShoppingList toDomain() =>
      ShoppingList(id: id, householdId: householdId, name: name);
}

class ShoppingListItemDto {
  const ShoppingListItemDto({
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
  factory ShoppingListItemDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final listId = json['shoppingListId'];
    final name = json['name'];
    final note = json['note'];
    final quantity = json['quantity'];
    final containerQuantity = json['containerQuantity'];
    final containerUnit = json['containerUnit'];
    final spaceId = json['spaceId'];
    final itemId = json['itemId'];
    final checked = json['checked'];
    final sortOrder = json['sortOrder'];
    if (id is! String ||
        listId is! String ||
        name is! String ||
        (note != null && note is! String) ||
        (quantity != null && quantity is! num) ||
        (containerQuantity != null && containerQuantity is! num) ||
        (containerUnit != null && containerUnit is! String) ||
        (spaceId != null && spaceId is! String) ||
        (itemId != null && itemId is! String) ||
        checked is! bool ||
        sortOrder is! int) {
      throw const FormatException();
    }
    return ShoppingListItemDto(
      id: id,
      shoppingListId: listId,
      name: name,
      note: note as String?,
      quantity: quantity as num?,
      containerQuantity: containerQuantity as num?,
      containerUnit: containerUnit as String?,
      spaceId: spaceId as String?,
      itemId: itemId as String?,
      checked: checked,
      sortOrder: sortOrder,
    );
  }
  ShoppingListItem toDomain() => ShoppingListItem(
    id: id,
    shoppingListId: shoppingListId,
    name: name,
    note: note,
    quantity: quantity,
    containerQuantity: containerQuantity,
    containerUnit: containerUnit,
    spaceId: spaceId,
    itemId: itemId,
    checked: checked,
    sortOrder: sortOrder,
  );
}
