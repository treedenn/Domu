import 'package:flutter/material.dart';

class DateField extends StatelessWidget {
  const DateField({
    required this.labelText,
    required this.value,
    required this.onChanged,
    required this.firstDate,
    required this.lastDate,
    this.clearable = false,
    super.key,
  });

  final String labelText;
  final DateTime? value;
  final DateTime firstDate;
  final DateTime lastDate;
  final bool clearable;
  final ValueChanged<DateTime?> onChanged;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: () async {
        final DateTime? picked = await showDatePicker(
          context: context,
          firstDate: firstDate,
          lastDate: lastDate,
          initialDate: value ?? DateTime.now(),
        );
        if (picked != null) {
          onChanged(picked);
        }
      },
      borderRadius: BorderRadius.circular(12),
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: labelText,
          suffixIcon: clearable && value != null
              ? IconButton(
                  tooltip: 'Clear date',
                  onPressed: () => onChanged(null),
                  icon: const Icon(Icons.close),
                )
              : const Icon(Icons.calendar_today_outlined),
        ),
        child: Text(
          value == null ? 'Not set' : '${value!.month}/${value!.day}/${value!.year}',
        ),
      ),
    );
  }
}
