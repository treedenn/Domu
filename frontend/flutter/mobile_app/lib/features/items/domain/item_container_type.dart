enum ItemContainerType { unspecified, bottle, carton, can, jar, pack, box, bag }

extension ItemContainerTypeX on ItemContainerType {
  static ItemContainerType fromJson(Object? value) {
    return switch (value) {
      String text when text.toLowerCase() == 'bottle' =>
        ItemContainerType.bottle,
      String text when text.toLowerCase() == 'carton' =>
        ItemContainerType.carton,
      String text when text.toLowerCase() == 'can' => ItemContainerType.can,
      String text when text.toLowerCase() == 'jar' => ItemContainerType.jar,
      String text when text.toLowerCase() == 'pack' => ItemContainerType.pack,
      String text when text.toLowerCase() == 'box' => ItemContainerType.box,
      String text when text.toLowerCase() == 'bag' => ItemContainerType.bag,
      1 => ItemContainerType.bottle,
      2 => ItemContainerType.carton,
      3 => ItemContainerType.can,
      4 => ItemContainerType.jar,
      5 => ItemContainerType.pack,
      6 => ItemContainerType.box,
      7 => ItemContainerType.bag,
      _ => ItemContainerType.unspecified,
    };
  }

  int toJson() {
    return switch (this) {
      ItemContainerType.unspecified => 0,
      ItemContainerType.bottle => 1,
      ItemContainerType.carton => 2,
      ItemContainerType.can => 3,
      ItemContainerType.jar => 4,
      ItemContainerType.pack => 5,
      ItemContainerType.box => 6,
      ItemContainerType.bag => 7,
    };
  }

  String get label {
    return switch (this) {
      ItemContainerType.unspecified => 'None',
      ItemContainerType.bottle => 'Bottle',
      ItemContainerType.carton => 'Carton',
      ItemContainerType.can => 'Can',
      ItemContainerType.jar => 'Jar',
      ItemContainerType.pack => 'Pack',
      ItemContainerType.box => 'Box',
      ItemContainerType.bag => 'Bag',
    };
  }

  String get shortLabel {
    return switch (this) {
      ItemContainerType.unspecified => '',
      ItemContainerType.bottle => 'bottle',
      ItemContainerType.carton => 'carton',
      ItemContainerType.can => 'can',
      ItemContainerType.jar => 'jar',
      ItemContainerType.pack => 'pack',
      ItemContainerType.box => 'box',
      ItemContainerType.bag => 'bag',
    };
  }
}
