import 'package:flutter/material.dart';

class SpacesView extends StatelessWidget {
  const SpacesView({super.key});

  @override
  Widget build(BuildContext context) => const Center(
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [Text('Spaces'), SizedBox(height: 8), Text('Coming soon.')],
    ),
  );
}
