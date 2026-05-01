import 'package:flutter/material.dart';

class QuantityStepper extends StatelessWidget {
  const QuantityStepper({
    required this.value,
    required this.onChanged,
    this.min = 0,
    this.max,
    super.key,
  });

  final int value;
  final int min;
  final int? max;
  final ValueChanged<int> onChanged;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: <Widget>[
        IconButton.outlined(
          onPressed: value <= min ? null : () => onChanged(value - 1),
          tooltip: 'Decrease quantity',
          icon: const Icon(Icons.remove),
        ),
        SizedBox(
          width: 56,
          child: Text(
            '$value',
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.titleMedium,
          ),
        ),
        IconButton.filledTonal(
          onPressed: max != null && value >= max! ? null : () => onChanged(value + 1),
          tooltip: 'Increase quantity',
          icon: const Icon(Icons.add),
        ),
      ],
    );
  }
}
