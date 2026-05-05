import 'consumable_state.dart';
import 'item_entry.dart';
import 'item_unit.dart';

class Item {
  const Item({
    required this.id,
    required this.spaceId,
    required this.name,
    required this.barcode,
    required this.totalQuantity,
    required this.earliestExpiresAt,
    required this.dominantState,
    required this.entryCount,
    required this.entries,
  });

  final String id;
  final String spaceId;
  final String name;
  final String? barcode;
  final double totalQuantity;
  final DateTime? earliestExpiresAt;
  final ConsumableState dominantState;
  final int entryCount;
  final List<ItemEntry> entries;

  String get quantityLabel {
    if (entries.isEmpty) {
      return '0 pc';
    }

    final Set<String> unitLabels = entries
        .map((ItemEntry entry) => entry.unit.shortLabel)
        .where((String label) => label.isNotEmpty)
        .toSet();
    if (unitLabels.length != 1) {
      return 'Mixed units';
    }

    return '${formatQuantity(totalQuantity)} ${unitLabels.single}';
  }

  factory Item.fromEntries({
    required String id,
    required String spaceId,
    required String name,
    required String? barcode,
    required List<ItemEntry> entries,
  }) {
    final List<ItemEntry> datedEntries =
        entries
            .where((ItemEntry entry) => entry.expiresAt != null)
            .toList(growable: false)
          ..sort(
            (ItemEntry a, ItemEntry b) => a.expiresAt!.compareTo(b.expiresAt!),
          );

    return Item(
      id: id,
      spaceId: spaceId,
      name: name,
      barcode: barcode,
      totalQuantity: entries.fold<double>(
        0,
        (double total, ItemEntry entry) => total + entry.currentQuantity,
      ),
      earliestExpiresAt: datedEntries.isEmpty
          ? null
          : datedEntries.first.expiresAt,
      dominantState: datedEntries.isEmpty
          ? ConsumableState.unknown
          : datedEntries.first.state,
      entryCount: entries.length,
      entries: List<ItemEntry>.unmodifiable(entries),
    );
  }

  static String formatQuantity(double value) {
    if (value == value.roundToDouble()) {
      return value.toStringAsFixed(0);
    }
    var text = value.toStringAsFixed(3);
    while (text.endsWith('0')) {
      text = text.substring(0, text.length - 1);
    }
    if (text.endsWith('.')) {
      text = text.substring(0, text.length - 1);
    }
    return text;
  }

  factory Item.fromJson(Map<String, Object?> json) {
    final String id = json['id'].toString();
    final Object? entriesJson = json['entries'];
    final List<ItemEntry> entries = entriesJson is List<Object?>
        ? entriesJson
              .whereType<Map<String, Object?>>()
              .map(
                (Map<String, Object?> entryJson) =>
                    ItemEntry.fromJson(itemId: id, json: entryJson),
              )
              .toList(growable: false)
        : const <ItemEntry>[];

    return Item.fromEntries(
      id: id,
      spaceId: json['spaceId'].toString(),
      name: json['name']?.toString() ?? 'Untitled item',
      barcode: json['barcode'] as String?,
      entries: entries,
    );
  }
}
