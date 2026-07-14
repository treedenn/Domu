import 'package:flutter/material.dart';

import '../domain/household.dart';
import 'households_view_model.dart';

class HouseholdsView extends StatefulWidget {
  const HouseholdsView({super.key, required this.viewModel, this.onSignOut});

  final HouseholdsViewModel viewModel;
  final Future<void> Function()? onSignOut;

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
          return ListTile(
            key: ValueKey('household-${household.id}'),
            selected: selected,
            leading: Icon(selected ? Icons.check_circle : Icons.home_outlined),
            title: Text(household.name),
            subtitle: selected ? const Text('Selected for this session') : null,
            onTap: () => viewModel.selectHousehold(household),
            trailing: PopupMenuButton<_HouseholdAction>(
              tooltip: 'Household actions',
              onSelected: (action) => switch (action) {
                _HouseholdAction.rename => _showRenameDialog(household),
                _HouseholdAction.delete => _showDeleteDialog(household),
              },
              itemBuilder: (context) => const [
                PopupMenuItem(
                  value: _HouseholdAction.rename,
                  child: Text('Rename'),
                ),
                PopupMenuItem(
                  value: _HouseholdAction.delete,
                  child: Text('Delete'),
                ),
              ],
            ),
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
  }) async {
    final name = TextEditingController(text: initialName);
    final owner = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final result = await showDialog<_HouseholdForm>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(title),
        content: Form(
          key: formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: name,
                autofocus: true,
                decoration: const InputDecoration(labelText: 'Household name'),
                validator: _required,
              ),
              if (includeOwner) ...[
                const SizedBox(height: 12),
                TextFormField(
                  controller: owner,
                  decoration: const InputDecoration(
                    labelText: 'Your display name',
                  ),
                  validator: _required,
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
                  includeOwner ? owner.text.trim() : null,
                ),
              );
            },
            child: Text(includeOwner ? 'Create' : 'Save'),
          ),
        ],
      ),
    );
    name.dispose();
    owner.dispose();
    return result;
  }

  String? _required(String? value) =>
      value == null || value.trim().isEmpty ? 'Required' : null;
}

enum _HouseholdAction { rename, delete }

class _HouseholdForm {
  const _HouseholdForm(this.name, this.ownerDisplayName);
  final String name;
  final String? ownerDisplayName;
}
