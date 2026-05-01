import 'package:flutter/material.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../shared/widgets/widgets.dart';

class HouseholdSettingsScreen extends StatefulWidget {
  const HouseholdSettingsScreen({
    required this.householdId,
    required this.householdName,
    this.onSignOut,
    super.key,
  });

  final String householdId;
  final String householdName;
  final Future<void> Function()? onSignOut;

  @override
  State<HouseholdSettingsScreen> createState() =>
      _HouseholdSettingsScreenState();
}

class _HouseholdSettingsScreenState extends State<HouseholdSettingsScreen> {
  late final TextEditingController _nameController;
  bool _expiryReminders = true;
  bool _lowStockAlerts = false;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.householdName);
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      children: <Widget>[
        AppCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text('Details', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: AppSpacing.md),
              TextField(
                controller: _nameController,
                decoration: const InputDecoration(labelText: 'Name'),
              ),
              const SizedBox(height: AppSpacing.md),
              const Wrap(
                spacing: AppSpacing.sm,
                children: <Widget>[
                  Chip(label: Text('Free')),
                  Chip(label: Text('Active')),
                ],
              ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        AppCard(
          child: Column(
            children: <Widget>[
              SwitchListTile(
                contentPadding: EdgeInsets.zero,
                title: const Text('Reminders for expiring items'),
                value: _expiryReminders,
                onChanged: (bool value) {
                  setState(() => _expiryReminders = value);
                },
              ),
              SwitchListTile(
                contentPadding: EdgeInsets.zero,
                title: const Text('Low stock alerts'),
                value: _lowStockAlerts,
                onChanged: (bool value) {
                  setState(() => _lowStockAlerts = value);
                },
              ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        AppCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              Text('Danger zone',
                  style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: AppSpacing.md),
              DestructiveButton(
                onPressed: () => _confirm(context, 'Leave household'),
                child: const Text('Leave household'),
              ),
              const SizedBox(height: AppSpacing.sm),
              DestructiveButton(
                onPressed: () => _confirm(context, 'Delete household'),
                child: const Text('Delete household'),
              ),
              if (widget.onSignOut != null) ...<Widget>[
                const SizedBox(height: AppSpacing.sm),
                OutlinedButton(
                  onPressed: widget.onSignOut,
                  child: const Text('Sign out'),
                ),
              ],
            ],
          ),
        ),
      ],
    );
  }

  Future<void> _confirm(BuildContext context, String title) {
    return showDialog<void>(
      context: context,
      builder: (BuildContext context) => AlertDialog(
        title: Text(title),
        content: Text('$title "${widget.householdName}"?'),
        actions: <Widget>[
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Confirm'),
          ),
        ],
      ),
    );
  }
}
