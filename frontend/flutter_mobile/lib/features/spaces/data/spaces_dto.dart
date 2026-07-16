import '../domain/space.dart';

class SpaceDto {
  const SpaceDto(this.json);
  final Map<String, dynamic> json;
  Space toDomain() => Space(
    id: json['id'] as String,
    householdId: json['householdId'] as String,
    parentId: json['parentId'] as String?,
    name: json['name'] as String,
    description: json['description'] as String?,
    itemCount: (json['items'] as Map?)?['count'] as int? ?? 0,
    childSpaceCount: (json['childSpaces'] as Map?)?['count'] as int? ?? 0,
  );
}

class SpaceItemDto {
  const SpaceItemDto(this.json);
  final Map<String, dynamic> json;
  SpaceItem toDomain() => SpaceItem(
    id: json['id'] as String,
    spaceId: json['spaceId'] as String,
    name: json['name'] as String,
    category: json['category'] as String?,
    barcode: json['barcode'] as String?,
    totalQuantity: json['totalQuantity'] as num,
    entries: ((json['entries'] as List?) ?? const [])
        .map(
          (entry) =>
              ItemEntryDto(Map<String, dynamic>.from(entry as Map)).toDomain(),
        )
        .toList(growable: false),
  );
}

class ItemEntryDto {
  const ItemEntryDto(this.json);
  final Map<String, dynamic> json;
  ItemEntry toDomain() => ItemEntry(
    id: json['id'] as String?,
    initialQuantity: json['initialQuantity'] as num,
    currentQuantity: json['currentQuantity'] as num,
    unit: ItemUnit.values.byName(json['unit'] as String),
    containerType: ItemContainerType.values.byName(
      json['containerType'] as String,
    ),
    state: ConsumableState.values.byName(json['state'] as String),
    acquisitionDate: _date(json['acquisitionDate'] as String?),
    expirationDate: _date(json['expirationDate'] as String?),
  );
  DateTime? _date(String? value) =>
      value == null ? null : DateTime.parse(value);
}
