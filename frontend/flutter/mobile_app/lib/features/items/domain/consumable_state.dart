enum ConsumableState { unknown, unopened, opened }

extension ConsumableStateX on ConsumableState {
  static ConsumableState fromJson(Object? value) {
    return switch (value) {
      String text when text.toLowerCase() == 'unopened' => ConsumableState.unopened,
      String text when text.toLowerCase() == 'opened' => ConsumableState.opened,
      String text when text.toLowerCase() == 'unknown' => ConsumableState.unknown,
      1 => ConsumableState.unopened,
      2 => ConsumableState.opened,
      _ => ConsumableState.unknown,
    };
  }

  int toJson() {
    return switch (this) {
      ConsumableState.unknown => 0,
      ConsumableState.unopened => 1,
      ConsumableState.opened => 2,
    };
  }
}
