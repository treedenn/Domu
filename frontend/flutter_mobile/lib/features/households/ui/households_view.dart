import 'package:flutter/material.dart';

import '../domain/household.dart';
import 'households_view_model.dart';
import 'widgets/household_tile.dart';

class HouseholdsView extends StatefulWidget {
  const HouseholdsView({
    super.key,
    required this.viewModel,
    this.onSignOut,
    this.onHouseholdSelected,
  });

  final HouseholdsViewModel viewModel;
  final Future<void> Function()? onSignOut;
  final void Function(Household household)? onHouseholdSelected;

  @override
  State<HouseholdsView> createState() => _HouseholdsViewState();
}

class _HouseholdsViewState extends State<HouseholdsView> {
  @override
  void initState() {
    super.initState();
    widget.viewModel.load();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: widget.viewModel,
      builder: (context, _) => Scaffold(
        appBar: AppBar(
          title: const Text('Your households'),
          actions: [
            IconButton(
              tooltip: 'Refresh',
              onPressed:
                  widget.viewModel.isRefreshing || widget.viewModel.isMutating
                  ? null
                  : widget.viewModel.refresh,
              icon: const Icon(Icons.refresh),
            ),
            IconButton(
              tooltip: 'Sign out',
              onPressed: widget.onSignOut,
              icon: const Icon(Icons.logout),
            ),
          ],
        ),
        floatingActionButton: FloatingActionButton.extended(
          onPressed: widget.viewModel.isMutating ? null : _showCreateDialog,
          icon: const Icon(Icons.add),
          label: const Text('Create household'),
        ),
        body: Column(
          children: [
            if (widget.viewModel.message case final message?)
              MaterialBanner(
                content: Text(message),
                actions: [
                  TextButton(
                    onPressed: widget.viewModel.clearMessage,
                    child: const Text('OK'),
                  ),
                ],
              ),
            Expanded(child: _buildBody()),
          ],
        ),
      ),
    );
  }

  Widget _buildBody() {
    final viewModel = widget.viewModel;
    if (viewModel.isLoading) {
      return const Center(child: CircularProgressIndicator());
    }
    if (viewModel.errorMessage case final error?) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(error, textAlign: TextAlign.center),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: viewModel.load,
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }
    if (viewModel.isEmpty) {
      return const Center(
        child: Text('No households yet. Create one to get started.'),
      );
    }
    return RefreshIndicator(
      onRefresh: viewModel.refresh,
      child: ListView.builder(
        itemCount: viewModel.households.length,
        itemBuilder: (context, index) {
          final household = viewModel.households[index];
          final selected = household.id == viewModel.selectedHouseholdId;
          return HouseholdTile(
            household: household,
            selected: selected,
            onSelected: (household) {
              viewModel.selectHousehold(household);
              widget.onHouseholdSelected?.call(household);
            },
            onRename: _showRenameDialog,
            onDelete: _showDeleteDialog,
          );
        },
      ),
    );
  }

  Future<void> _showCreateDialog() async {
    final result = await _showHouseholdDialog(
      title: 'Create household',
      includeOwner: true,
    );
    if (result == null) {
      return;
    }
    await widget.viewModel.createHousehold(
      name: result.name,
      ownerDisplayName: result.ownerDisplayName!,
    );
  }

  Future<void> _showRenameDialog(Household household) async {
    final result = await _showHouseholdDialog(
      title: 'Rename household',
      initialName: household.name,
    );
    if (result == null) return;
    await widget.viewModel.renameHousehold(id: household.id, name: result.name);
  }

  Future<void> _showDeleteDialog(Household household) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete household?'),
        content: Text('Delete ${household.name}? This cannot be undone.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (confirmed == true) await widget.viewModel.deleteHousehold(household.id);
  }

  Future<_HouseholdForm?> _showHouseholdDialog({
    required String title,
    String initialName = '',
    bool includeOwner = false,
  }) => showDialog<_HouseholdForm>(
    context: context,
    builder: (_) => _HouseholdDialog(
      title: title,
      initialName: initialName,
      includeOwner: includeOwner,
      validator: _required,
    ),
  );

  String? _required(String? value) =>
      value == null || value.trim().isEmpty ? 'Required' : null;
}

class _HouseholdForm {
  const _HouseholdForm(this.name, this.ownerDisplayName);
  final String name;
  final String? ownerDisplayName;
}

class _HouseholdDialog extends StatefulWidget {
  const _HouseholdDialog({
    required this.title,
    required this.initialName,
    required this.includeOwner,
    required this.validator,
  });

  final String title;
  final String initialName;
  final bool includeOwner;
  final String? Function(String?) validator;

  @override
  State<_HouseholdDialog> createState() => _HouseholdDialogState();
}

class _HouseholdDialogState extends State<_HouseholdDialog> {
  late final name = TextEditingController(text: widget.initialName);
  final owner = TextEditingController();
  final formKey = GlobalKey<FormState>();

  @override
  void dispose() {
    name.dispose();
    owner.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.title),
    content: Form(
      key: formKey,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: name,
            autofocus: true,
            decoration: const InputDecoration(labelText: 'Household name'),
            validator: widget.validator,
          ),
          if (widget.includeOwner) ...[
            const SizedBox(height: 12),
            TextFormField(
              controller: owner,
              decoration: const InputDecoration(labelText: 'Your display name'),
              validator: widget.validator,
            ),
          ],
        ],
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Cancel'),
      ),
      FilledButton(
        onPressed: () {
          if (!(formKey.currentState?.validate() ?? false)) return;
          Navigator.pop(
            context,
            _HouseholdForm(
              name.text.trim(),
              widget.includeOwner ? owner.text.trim() : null,
            ),
          );
        },
        child: Text(widget.includeOwner ? 'Create' : 'Save'),
      ),
    ],
  );
}
