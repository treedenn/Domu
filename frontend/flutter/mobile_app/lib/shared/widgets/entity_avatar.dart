import 'package:flutter/material.dart';

enum EntityAvatarSize { sm, md, lg }

class EntityAvatar extends StatelessWidget {
  const EntityAvatar({
    required this.id,
    required this.name,
    this.size = EntityAvatarSize.md,
    super.key,
  });

  final String id;
  final String name;
  final EntityAvatarSize size;

  double get _dimension {
    return switch (size) {
      EntityAvatarSize.sm => 32,
      EntityAvatarSize.md => 40,
      EntityAvatarSize.lg => 56,
    };
  }

  @override
  Widget build(BuildContext context) {
    final ColorScheme colorScheme = Theme.of(context).colorScheme;
    final Color background = _colorFor(colorScheme);

    return CircleAvatar(
      radius: _dimension / 2,
      backgroundColor: background,
      foregroundColor: _foregroundFor(background),
      child: Text(
        _initials(name),
        style: Theme.of(context).textTheme.labelLarge?.copyWith(
              fontWeight: FontWeight.w700,
              color: _foregroundFor(background),
            ),
      ),
    );
  }

  Color _colorFor(ColorScheme colorScheme) {
    final List<Color> palette = <Color>[
      colorScheme.primaryContainer,
      colorScheme.secondaryContainer,
      colorScheme.tertiaryContainer,
      colorScheme.surfaceContainerHighest,
    ];
    return palette[id.hashCode.abs() % palette.length];
  }

  Color _foregroundFor(Color background) {
    return background.computeLuminance() > 0.45 ? Colors.black : Colors.white;
  }

  String _initials(String value) {
    final List<String> parts = value
        .trim()
        .split(RegExp(r'\s+'))
        .where((String part) => part.isNotEmpty)
        .toList(growable: false);
    if (parts.isEmpty) {
      return '?';
    }
    return parts.take(2).map((String part) => part[0]).join().toUpperCase();
  }
}
