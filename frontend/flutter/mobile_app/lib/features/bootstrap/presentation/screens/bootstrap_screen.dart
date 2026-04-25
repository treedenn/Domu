import 'package:flutter/material.dart';

import '../../../../app/routing/app_router.dart';

class BootstrapScreen extends StatelessWidget {
  const BootstrapScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Domu')),
      body: Padding(
        padding: const EdgeInsets.all(24),
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
                    Navigator.of(context).pushNamed(AppRouter.householdsRoute);
                  },
                  child: const Text('Households'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    Navigator.of(context).pushNamed(AppRouter.householdsRoute);
                  },
                  child: const Text('Households'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    Navigator.of(context).pushNamed(AppRouter.spacesRoute);
                  },
                  child: const Text('Spaces'),
                ),
                FilledButton.tonal(
                  onPressed: () {
                    Navigator.of(context).pushNamed(AppRouter.usersRoute);
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
