import 'package:flutter/material.dart';

class ShoppingListsView extends StatelessWidget {
  const ShoppingListsView({super.key});

  @override
  Widget build(BuildContext context) => const Center(
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text('Shopping Lists'),
        SizedBox(height: 8),
        Text('Coming soon.'),
      ],
    ),
  );
}
