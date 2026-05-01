import 'package:flutter/material.dart';

import '../../app/theme/tokens.dart';

class FilterChipOption<T> {
  const FilterChipOption({required this.value, required this.label});

  final T value;
  final String label;
}

class FilterChipStrip<T> extends StatelessWidget {
  const FilterChipStrip({
    required this.options,
    required this.selected,
    required this.onSelected,
    required this.defaultValue,
    super.key,
  });

  final List<FilterChipOption<T>> options;
  final T selected;
  final T defaultValue;
  final ValueChanged<T> onSelected;

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: AppSpacing.sm,
      runSpacing: AppSpacing.sm,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: <Widget>[
        for (final FilterChipOption<T> option in options)
          FilterChip(
            selected: option.value == selected,
            onSelected: (_) => onSelected(option.value),
            label: Text(option.label),
          ),
        if (selected != defaultValue)
          TextButton(
            onPressed: () => onSelected(defaultValue),
            child: const Text('Clear'),
          ),
      ],
    );
  }
}
