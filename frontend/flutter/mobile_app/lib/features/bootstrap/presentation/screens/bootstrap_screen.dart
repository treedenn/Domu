import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../app/theme/tokens.dart';

class BootstrapScreen extends StatelessWidget {
  const BootstrapScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Domu')),
      body: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: <Widget>[
            Text(
              'Base application structure is ready.',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 12),
            Text(
              'Start wiring auth, bootstrap data, and feature repositories from here.',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Wrap(
              spacing: 12,
              runSpacing: 12,
              children: <Widget>[
                FilledButton(
                  onPressed: () {
                    context.go('/households');
                  },
                  child: const Text('Households'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    context.go('/households');
                  },
                  child: const Text('Households'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    context.go('/households/demo/spaces');
                  },
                  child: const Text('Spaces'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    context.go('/households/demo/members');
                  },
                  child: const Text('Users'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
