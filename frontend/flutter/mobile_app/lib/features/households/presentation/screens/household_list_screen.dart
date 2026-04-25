import 'package:flutter/material.dart';

import '../../../../core/ui/empty_view.dart';
import '../../../../shared/widgets/section_card.dart';

class HouseholdListScreen extends StatelessWidget {
  const HouseholdListScreen({
    this.onOpenSpaces,
    this.onOpenUsers,
    this.onSignOut,
    super.key,
  });

  final void Function(BuildContext context)? onOpenSpaces;
  final void Function(BuildContext context)? onOpenUsers;
  final Future<void> Function()? onSignOut;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Households'),
        actions: <Widget>[
          if (onSignOut != null)
            IconButton(
              onPressed: onSignOut,
              tooltip: 'Sign out',
              icon: const Icon(Icons.logout),
            ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: <Widget>[
          SectionCard(
            title: 'Authenticated',
            child: Text(
              'The ZITADEL login flow completed and the session is stored locally.',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
          ),
          const SizedBox(height: 16),
          SectionCard(
            title: 'Next screens',
            child: Wrap(
              spacing: 12,
              runSpacing: 12,
              children: <Widget>[
                FilledButton.tonal(
                  onPressed: onOpenSpaces == null
                      ? null
                      : () => onOpenSpaces!(context),
                  child: const Text('Open spaces'),
                ),
                FilledButton.tonal(
                  onPressed: onOpenUsers == null
                      ? null
                      : () => onOpenUsers!(context),
                  child: const Text('Open profile'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          const EmptyView(
            title: 'No households yet',
            message: 'Connect the households repository and render the overview here.',
          ),
        ],
      ),
    );
  }
}
