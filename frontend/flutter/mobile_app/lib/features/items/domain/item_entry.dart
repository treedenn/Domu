import 'consumable_state.dart';

class ItemEntry {
  const ItemEntry({
    required this.id,
    required this.itemId,
    required this.quantity,
    required this.acquiredAt,
    required this.expiresAt,
    required this.state,
  });

  final String id;
  final String itemId;
  final int quantity;
  final DateTime acquiredAt;
  final DateTime? expiresAt;
  final ConsumableState state;

  ItemEntry copyWith({
    String? id,
    String? itemId,
    int? quantity,
    DateTime? acquiredAt,
    DateTime? expiresAt,
    ConsumableState? state,
  }) {
    return ItemEntry(
      id: id ?? this.id,
      itemId: itemId ?? this.itemId,
      quantity: quantity ?? this.quantity,
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
      quantity: json['quantity'] as int? ?? 0,
      acquiredAt: _date(json['acquisitionDate']) ?? DateTime.now(),
      expiresAt: _date(json['expirationDate']),
      state: ConsumableStateX.fromJson(json['state']),
    );
  }

  Map<String, Object?> toJson() {
    return <String, Object?>{
      'id': id.isEmpty ? null : id,
      'quantity': quantity,
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
}
