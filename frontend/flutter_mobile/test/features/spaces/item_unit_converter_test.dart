import 'package:domu_mobile/features/spaces/domain/item_unit_converter.dart';
import 'package:domu_mobile/features/spaces/domain/space.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('converts item amounts using the configured thousand-unit rules', () {
    expect(
      ItemUnitConverter.convert(
        2,
        from: ItemUnit.piece,
        to: ItemUnit.milliliter,
      ),
      2000,
    );
    expect(
      ItemUnitConverter.convert(
        3,
        from: ItemUnit.liter,
        to: ItemUnit.milliliter,
      ),
      3000,
    );
    expect(
      ItemUnitConverter.convert(2, from: ItemUnit.kilogram, to: ItemUnit.gram),
      2000,
    );
  });

  test('converts in reverse and leaves matching units unchanged', () {
    expect(
      ItemUnitConverter.convert(
        1000,
        from: ItemUnit.milliliter,
        to: ItemUnit.liter,
      ),
      1,
    );
    expect(
      ItemUnitConverter.convert(4, from: ItemUnit.gram, to: ItemUnit.gram),
      4,
    );
  });

  test('keeps the amount unchanged for unlisted unit pairs', () {
    expect(
      ItemUnitConverter.convert(1, from: ItemUnit.gram, to: ItemUnit.liter),
      1,
    );
    expect(
      ItemUnitConverter.convert(1, from: ItemUnit.piece, to: ItemUnit.liter),
      1,
    );
  });
}
