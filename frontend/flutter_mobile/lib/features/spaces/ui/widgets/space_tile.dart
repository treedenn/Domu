import 'package:flutter/material.dart';

import '../../domain/space.dart';

class SpaceTile extends StatelessWidget {
  const SpaceTile({super.key, required this.space, required this.onTap});

  final Space space;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => ListTile(
    leading: const Icon(Icons.folder_outlined),
    title: Text(space.name),
    subtitle: Text(
      '${space.childSpaceCount} subspaces · ${space.itemCount} items',
    ),
    trailing: const Icon(Icons.chevron_right),
    onTap: onTap,
  );
}
