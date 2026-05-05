import 'consumable_state.dart';
import 'item_container_type.dart';
import 'item_unit.dart';

class ItemEntry {
  const ItemEntry({
    required this.id,
    required this.itemId,
    required this.initialQuantity,
    required this.currentQuantity,
    required this.unit,
    required this.containerType,
    required this.acquiredAt,
    required this.expiresAt,
    required this.state,
  });

  final String id;
  final String itemId;
  final double initialQuantity;
  final double currentQuantity;
  final ItemUnit unit;
  final ItemContainerType containerType;
  final DateTime acquiredAt;
  final DateTime? expiresAt;
  final ConsumableState state;

  ItemEntry copyWith({
    String? id,
    String? itemId,
    double? initialQuantity,
    double? currentQuantity,
    ItemUnit? unit,
    ItemContainerType? containerType,
    DateTime? acquiredAt,
    DateTime? expiresAt,
    ConsumableState? state,
  }) {
    return ItemEntry(
      id: id ?? this.id,
      itemId: itemId ?? this.itemId,
      initialQuantity: initialQuantity ?? this.initialQuantity,
      currentQuantity: currentQuantity ?? this.currentQuantity,
      unit: unit ?? this.unit,
      containerType: containerType ?? this.containerType,
      acquiredAt: acquiredAt ?? this.acquiredAt,
      expiresAt: expiresAt ?? this.expiresAt,
      state: state ?? this.state,
    );
  }

  factory ItemEntry.fromJson({
    required String itemId,
    required Map<String, Object?> json,
  }) {
    return ItemEntry(
      id: json['id'].toString(),
      itemId: itemId,
      initialQuantity: _number(json['initialQuantity']) ?? 0,
      currentQuantity:
          _number(json['currentQuantity']) ?? _number(json['quantity']) ?? 0,
      unit: ItemUnitX.fromJson(json['unit']),
      containerType: ItemContainerTypeX.fromJson(json['containerType']),
      acquiredAt: _date(json['acquisitionDate']) ?? DateTime.now(),
      expiresAt: _date(json['expirationDate']),
      state: ConsumableStateX.fromJson(json['state']),
    );
  }

  Map<String, Object?> toJson() {
    return <String, Object?>{
      'id': id.isEmpty ? null : id,
      'initialQuantity': initialQuantity,
      'currentQuantity': currentQuantity,
      'unit': unit.toJson(),
      'containerType': containerType.toJson(),
      'state': state.toJson(),
      'acquisitionDate': acquiredAt.toUtc().toIso8601String(),
      'expirationDate': expiresAt?.toUtc().toIso8601String(),
    };
  }

  static DateTime? _date(Object? value) {
    if (value is String && value.isNotEmpty) {
      return DateTime.tryParse(value)?.toLocal();
    }
    return null;
  }

  static double? _number(Object? value) {
    if (value is num) {
      return value.toDouble();
    }
    if (value is String) {
      return double.tryParse(value);
    }
    return null;
  }
}
