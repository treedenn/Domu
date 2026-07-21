import 'package:flutter/material.dart';

import '../../domain/space.dart';

class MoveSpaceDialog extends StatelessWidget {
  const MoveSpaceDialog({super.key, required this.destinations});

  static const cancelled = '__cancelled__';
  static const root = '__root__';
  final List<Space> destinations;

  @override
  Widget build(BuildContext context) => SimpleDialog(
    title: const Text('Move space to'),
    children: [
      SimpleDialogOption(
        onPressed: () => Navigator.pop(context, root),
        child: const Text('Top level'),
      ),
      ...destinations.map(
        (space) => SimpleDialogOption(
          onPressed: () => Navigator.pop(context, space.id),
          child: Text(space.name),
        ),
      ),
      SimpleDialogOption(
        onPressed: () => Navigator.pop(context, cancelled),
        child: const Text('Cancel'),
      ),
    ],
  );
}
