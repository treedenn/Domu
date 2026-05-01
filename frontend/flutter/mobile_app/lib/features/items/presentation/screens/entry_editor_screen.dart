import 'package:flutter/material.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item_entry.dart';

class EntryEditorScreen extends StatefulWidget {
  const EntryEditorScreen({
    required this.itemId,
    required this.repository,
    required this.householdId,
    required this.spaceId,
    this.session,
    this.entryId,
    super.key,
  });

  final String householdId;
  final String spaceId;
  final String itemId;
  final String? entryId;
  final ItemsRepository repository;
  final AuthSession? session;

  @override
  State<EntryEditorScreen> createState() => _EntryEditorScreenState();
}

class _EntryEditorScreenState extends State<EntryEditorScreen> {
  int _quantity = 1;
  DateTime _acquiredAt = DateTime.now();
  DateTime? _expiresAt;
  ConsumableState _state = ConsumableState.unknown;
  String? _error;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.lg),
        children: <Widget>[
          AppCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text('Quantity', style: Theme.of(context).textTheme.titleMedium),
                const SizedBox(height: AppSpacing.md),
                QuantityStepper(
                  value: _quantity,
                  min: 1,
                  onChanged: (int value) => setState(() => _quantity = value),
                ),
              ],
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          AppCard(
            child: Column(
              children: <Widget>[
                DateField(
                  labelText: 'Acquired on',
                  value: _acquiredAt,
                  onChanged: (DateTime? value) {
                    if (value != null) {
                      setState(() => _acquiredAt = value);
                    }
                  },
                  firstDate: DateTime(2000),
                  lastDate: DateTime.now(),
                ),
                const SizedBox(height: AppSpacing.md),
                DateField(
                  labelText: 'Expires on',
                  value: _expiresAt,
                  clearable: true,
                  onChanged: (DateTime? value) => setState(() => _expiresAt = value),
                  firstDate: DateTime(2000),
                  lastDate: DateTime(2100),
                ),
              ],
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          AppCard(
            child: SegmentedButton<ConsumableState>(
              segments: const <ButtonSegment<ConsumableState>>[
                ButtonSegment<ConsumableState>(
                  value: ConsumableState.unknown,
                  label: Text('Unknown'),
                ),
                ButtonSegment<ConsumableState>(
                  value: ConsumableState.unopened,
                  label: Text('Unopened'),
                ),
                ButtonSegment<ConsumableState>(
                  value: ConsumableState.opened,
                  label: Text('Opened'),
                ),
              ],
              selected: <ConsumableState>{_state},
              onSelectionChanged: (Set<ConsumableState> value) {
                setState(() => _state = value.first);
              },
            ),
          ),
          if (_error != null) ...<Widget>[
            const SizedBox(height: AppSpacing.md),
            Text(
              _error!,
              style: TextStyle(color: Theme.of(context).colorScheme.error),
            ),
          ],
        ],
      ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Row(
            children: <Widget>[
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.of(context).pop(),
                  child: const Text('Cancel'),
                ),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: FilledButton(
                  onPressed: _save,
                  child: const Text('Save'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _save() async {
    if (_expiresAt != null && _expiresAt!.isBefore(_acquiredAt)) {
      setState(() => _error = 'Expiration must be after acquired date.');
      return;
    }
    final AuthSession? session = widget.session;
    if (session == null) {
      setState(() => _error = 'You need to sign in before saving.');
      return;
    }
    await widget.repository.saveEntry(
      session: session,
      householdId: widget.householdId,
      spaceId: widget.spaceId,
      entry: ItemEntry(
        id: widget.entryId ?? '',
        itemId: widget.itemId,
        quantity: _quantity,
        acquiredAt: _acquiredAt,
        expiresAt: _expiresAt,
        state: _state,
      ),
    );
    if (mounted) {
      Navigator.of(context).pop();
    }
  }
}
