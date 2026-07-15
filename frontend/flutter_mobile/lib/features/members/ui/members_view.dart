import 'package:flutter/material.dart';

import '../domain/household_member.dart';
import 'members_view_model.dart';

class MembersView extends StatefulWidget {
  const MembersView({
    super.key,
    required this.householdId,
    required this.viewModel,
  });

  final String householdId;
  final MembersViewModel viewModel;

  @override
  State<MembersView> createState() => _MembersViewState();
}

class _MembersViewState extends State<MembersView> {
  @override
  void initState() {
    super.initState();
    widget.viewModel.load(widget.householdId);
  }

  @override
  void didUpdateWidget(covariant MembersView oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.householdId != widget.householdId) {
      widget.viewModel.load(widget.householdId);
    }
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
    animation: widget.viewModel,
    builder: (context, _) => Scaffold(
      floatingActionButton: widget.viewModel.canManageMembers
          ? FloatingActionButton.extended(
              onPressed: widget.viewModel.isMutating ? null : _showInviteDialog,
              icon: const Icon(Icons.person_add),
              label: const Text('Add member'),
            )
          : null,
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
                onPressed: () => viewModel.load(widget.householdId),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: viewModel.refresh,
      child: viewModel.isEmpty && !viewModel.canManageMembers
          ? ListView(
              children: const [
                SizedBox(height: 240),
                Center(child: Text('No active members yet.')),
              ],
            )
          : ListView(
              children: [
                if (viewModel.members.isEmpty)
                  const Padding(
                    padding: EdgeInsets.all(24),
                    child: Center(child: Text('No active members yet.')),
                  )
                else
                  ...viewModel.members.map(_memberTile),
                if (viewModel.canManageMembers) ...[
                  const Divider(height: 32),
                  const Padding(
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    child: Text(
                      'Pending invitations',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  if (viewModel.pendingInvitations.isEmpty)
                    const Padding(
                      padding: EdgeInsets.fromLTRB(16, 8, 16, 24),
                      child: Text('No pending invitations.'),
                    )
                  else
                    ...viewModel.pendingInvitations.map(
                      (invitation) => ListTile(
                        key: ValueKey('invitation-${invitation.id}'),
                        leading: const Icon(Icons.mail_outline),
                        title: Text(invitation.displayName),
                        subtitle: Text(
                          '${invitation.email} · ${invitation.role.label} · Pending',
                        ),
                      ),
                    ),
                ],
              ],
            ),
    );
  }

  Widget _memberTile(HouseholdMember member) => ListTile(
    key: ValueKey('member-${member.id}'),
    leading: Icon(
      member.role == HouseholdMemberRole.owner
          ? Icons.workspace_premium_outlined
          : Icons.person_outline,
    ),
    title: Text(member.displayName),
    subtitle: Text(member.role.label),
    trailing:
        widget.viewModel.canManageMembers &&
            member.role != HouseholdMemberRole.owner
        ? PopupMenuButton<_MemberAction>(
            tooltip: 'Member actions',
            onSelected: (_) => _showArchiveDialog(member),
            itemBuilder: (context) => const [
              PopupMenuItem(value: _MemberAction.remove, child: Text('Remove')),
            ],
          )
        : null,
  );

  Future<void> _showArchiveDialog(HouseholdMember member) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Remove member?'),
        content: Text('Remove ${member.displayName} from this household?'),
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
            child: const Text('Remove'),
          ),
        ],
      ),
    );
    if (confirmed == true) await widget.viewModel.archive(member);
  }

  Future<void> _showInviteDialog() async {
    final form = await _showInviteForm();
    if (form == null) return;
    await widget.viewModel.invite(
      displayName: form.displayName,
      email: form.email,
      role: form.role,
    );
  }

  Future<_InvitationForm?> _showInviteForm() async {
    final displayName = TextEditingController();
    final email = TextEditingController();
    final formKey = GlobalKey<FormState>();
    var role = HouseholdMemberRole.member;
    final result = await showDialog<_InvitationForm>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Add member'),
          content: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextFormField(
                  controller: displayName,
                  autofocus: true,
                  decoration: const InputDecoration(labelText: 'Display name'),
                  validator: _required,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: email,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(labelText: 'Email'),
                  validator: _email,
                ),
                const SizedBox(height: 12),
                DropdownButtonFormField<HouseholdMemberRole>(
                  initialValue: role,
                  decoration: const InputDecoration(labelText: 'Role'),
                  items: const [
                    DropdownMenuItem(
                      value: HouseholdMemberRole.member,
                      child: Text('Member'),
                    ),
                    DropdownMenuItem(
                      value: HouseholdMemberRole.admin,
                      child: Text('Admin'),
                    ),
                  ],
                  onChanged: (value) {
                    if (value != null) setDialogState(() => role = value);
                  },
                ),
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
                  _InvitationForm(
                    displayName.text.trim(),
                    email.text.trim(),
                    role,
                  ),
                );
              },
              child: const Text('Send invitation'),
            ),
          ],
        ),
      ),
    );
    displayName.dispose();
    email.dispose();
    return result;
  }

  String? _required(String? value) =>
      value == null || value.trim().isEmpty ? 'Required' : null;
  String? _email(String? value) {
    if (value == null || value.trim().isEmpty) return 'Required';
    return RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(value.trim())
        ? null
        : 'Enter a valid email';
  }
}

enum _MemberAction { remove }

class _InvitationForm {
  const _InvitationForm(this.displayName, this.email, this.role);
  final String displayName;
  final String email;
  final HouseholdMemberRole role;
}
