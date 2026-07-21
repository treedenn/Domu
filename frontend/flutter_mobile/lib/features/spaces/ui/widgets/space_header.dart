import 'package:flutter/material.dart';

import '../../domain/space.dart';

class SpaceHeader extends StatelessWidget {
  const SpaceHeader({
    super.key,
    required this.space,
    required this.path,
    required this.onNavigate,
    required this.onEdit,
    required this.onMove,
    required this.onDelete,
  });

  final Space space;
  final List<Space> path;
  final ValueChanged<String?> onNavigate;
  final ValueChanged<Space> onEdit;
  final ValueChanged<Space> onMove;
  final ValueChanged<Space> onDelete;

  @override
  Widget build(BuildContext context) {
    final parent = path.length > 1 ? path[path.length - 2] : null;
    return Material(
      color: Theme.of(context).colorScheme.surfaceContainerLow,
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            IconButton(
              tooltip: parent == null
                  ? 'Back to Spaces'
                  : 'Back to ${parent.name}',
              onPressed: () => onNavigate(parent?.id),
              icon: const Icon(Icons.arrow_back),
            ),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (path.length > 1) ...[
                    Wrap(
                      crossAxisAlignment: WrapCrossAlignment.center,
                      spacing: 2,
                      children: [
                        for (
                          var index = 0;
                          index < path.length - 1;
                          index++
                        ) ...[
                          if (index > 0)
                            const Icon(Icons.chevron_right, size: 18),
                          TextButton(
                            onPressed: () => onNavigate(path[index].id),
                            child: Text(path[index].name),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                  ],
                  Text(
                    parent == null ? 'Main space' : 'Subspace',
                    style: Theme.of(context).textTheme.labelLarge,
                  ),
                  Text(
                    space.name,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  if (space.description case final description?
                      when description.isNotEmpty)
                    Text(description),
                ],
              ),
            ),
            PopupMenuButton<_SpaceAction>(
              onSelected: (action) => switch (action) {
                _SpaceAction.edit => onEdit(space),
                _SpaceAction.move => onMove(space),
                _SpaceAction.delete => onDelete(space),
              },
              itemBuilder: (_) => const [
                PopupMenuItem(
                  value: _SpaceAction.edit,
                  child: Text('Edit space'),
                ),
                PopupMenuItem(
                  value: _SpaceAction.move,
                  child: Text('Move space'),
                ),
                PopupMenuItem(
                  value: _SpaceAction.delete,
                  child: Text('Delete space'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

enum _SpaceAction { edit, move, delete }
