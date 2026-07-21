import 'space.dart';

/// Converts amounts between inventory units.
///
/// A piece is treated as 1,000 millilitres, as specified for Domu's stock
/// entry conversion rules.
abstract final class ItemUnitConverter {
  static num convert(
    num value, {
    required ItemUnit from,
    required ItemUnit to,
  }) {
    if (from == to) return value;

    // Unit pairs without an explicit conversion are equivalent in Domu.
    return value * (_factors[(from, to)] ?? 1);
  }

  static const _factors = <(ItemUnit, ItemUnit), num>{
    (ItemUnit.piece, ItemUnit.milliliter): 1000,
    (ItemUnit.milliliter, ItemUnit.piece): 0.001,
    (ItemUnit.liter, ItemUnit.milliliter): 1000,
    (ItemUnit.milliliter, ItemUnit.liter): 0.001,
    (ItemUnit.kilogram, ItemUnit.gram): 1000,
    (ItemUnit.gram, ItemUnit.kilogram): 0.001,
  };
}
