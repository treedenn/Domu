import 'package:flutter/material.dart';

import '../../domain/space.dart';

class SpaceFormValues {
  const SpaceFormValues(this.name, this.description);
  final String name;
  final String? description;
}

class SpaceFormDialog extends StatefulWidget {
  const SpaceFormDialog({super.key, this.space});
  final Space? space;

  @override
  State<SpaceFormDialog> createState() => _SpaceFormDialogState();
}

class _SpaceFormDialogState extends State<SpaceFormDialog> {
  late final name = TextEditingController(text: widget.space?.name);
  late final description = TextEditingController(
    text: widget.space?.description,
  );
  final key = GlobalKey<FormState>();

  @override
  void dispose() {
    name.dispose();
    description.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.space == null ? 'New space' : 'Edit space'),
    content: Form(
      key: key,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: name,
            autofocus: true,
            maxLength: 100,
            decoration: const InputDecoration(labelText: 'Name'),
            validator: _required,
          ),
          TextFormField(
            controller: description,
            maxLength: 255,
            minLines: 1,
            maxLines: 3,
            decoration: const InputDecoration(
              labelText: 'Description (optional)',
            ),
          ),
        ],
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: () {
          if (key.currentState!.validate()) {
            Navigator.pop(
              context,
              SpaceFormValues(name.text.trim(), _optional(description.text)),
            );
          }
        },
        child: const Text('Save'),
      ),
    ],
  );
}

String? _required(String? value) =>
    value?.trim().isEmpty ?? true ? 'Required' : null;
String? _optional(String value) => value.trim().isEmpty ? null : value.trim();
