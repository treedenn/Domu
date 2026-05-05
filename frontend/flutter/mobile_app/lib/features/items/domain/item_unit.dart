enum ItemUnitKind { unspecified, count, volume, mass }

enum ItemUnit { unspecified, piece, milliliter, liter, gram, kilogram }

extension ItemUnitX on ItemUnit {
  static ItemUnit fromJson(Object? value) {
    return switch (value) {
      String text when text.toLowerCase() == 'piece' => ItemUnit.piece,
      String text when text.toLowerCase() == 'milliliter' =>
        ItemUnit.milliliter,
      String text when text.toLowerCase() == 'liter' => ItemUnit.liter,
      String text when text.toLowerCase() == 'gram' => ItemUnit.gram,
      String text when text.toLowerCase() == 'kilogram' => ItemUnit.kilogram,
      1 => ItemUnit.piece,
      100 => ItemUnit.milliliter,
      101 => ItemUnit.liter,
      200 => ItemUnit.gram,
      201 => ItemUnit.kilogram,
      _ => ItemUnit.piece,
    };
  }

  int toJson() {
    return switch (this) {
      ItemUnit.unspecified => 0,
      ItemUnit.piece => 1,
      ItemUnit.milliliter => 100,
      ItemUnit.liter => 101,
      ItemUnit.gram => 200,
      ItemUnit.kilogram => 201,
    };
  }

  ItemUnitKind get kind {
    return switch (this) {
      ItemUnit.unspecified => ItemUnitKind.unspecified,
      ItemUnit.piece => ItemUnitKind.count,
      ItemUnit.milliliter || ItemUnit.liter => ItemUnitKind.volume,
      ItemUnit.gram || ItemUnit.kilogram => ItemUnitKind.mass,
    };
  }

  String get label {
    return switch (this) {
      ItemUnit.unspecified => 'Unspecified',
      ItemUnit.piece => 'Piece',
      ItemUnit.milliliter => 'Milliliter',
      ItemUnit.liter => 'Liter',
      ItemUnit.gram => 'Gram',
      ItemUnit.kilogram => 'Kilogram',
    };
  }

  String get shortLabel {
    return switch (this) {
      ItemUnit.unspecified => '',
      ItemUnit.piece => 'pc',
      ItemUnit.milliliter => 'ml',
      ItemUnit.liter => 'L',
      ItemUnit.gram => 'g',
      ItemUnit.kilogram => 'kg',
    };
  }
}

extension ItemUnitKindX on ItemUnitKind {
  String get label {
    return switch (this) {
      ItemUnitKind.unspecified => 'Unspecified',
      ItemUnitKind.count => 'Count',
      ItemUnitKind.volume => 'Volume',
      ItemUnitKind.mass => 'Mass',
    };
  }
}
