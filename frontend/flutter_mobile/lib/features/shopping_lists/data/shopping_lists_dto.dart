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
  factory ShoppingListItemDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final listId = json['shoppingListId'];
    final name = json['name'];
    final note = json['note'];
    final count = json['count'];
    final amountPerUnit = json['amountPerUnit'];
    final unit = json['unit'];
    final spaceId = json['spaceId'];
    final itemId = json['itemId'];
    final checked = json['checked'];
    final sortOrder = json['sortOrder'];
    if (id is! String ||
        listId is! String ||
        name is! String ||
        (note != null && note is! String) ||
        count is! int ||
        (amountPerUnit != null && amountPerUnit is! num) ||
        (unit != null && unit is! String) ||
        (spaceId != null && spaceId is! String) ||
        (itemId != null && itemId is! String) ||
        checked is! bool ||
        sortOrder is! num) {
      throw const FormatException();
    }
    return ShoppingListItemDto(
      id: id,
      shoppingListId: listId,
      name: name,
      note: note as String?,
      count: count,
      amountPerUnit: amountPerUnit as num?,
      unit: unit == null ? null : ShoppingListItemUnit.values.byName(unit),
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
    count: count,
    amountPerUnit: amountPerUnit,
    unit: unit,
    spaceId: spaceId,
    itemId: itemId,
    checked: checked,
    sortOrder: sortOrder,
  );
}
