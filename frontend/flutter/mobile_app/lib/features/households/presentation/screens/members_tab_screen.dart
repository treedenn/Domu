import 'package:flutter/material.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../core/auth/auth_session.dart';
import '../../../../core/ui/empty_view.dart';
import '../../../../core/ui/error_view.dart';
import '../../../../core/ui/loading_view.dart';
import '../../../../shared/widgets/widgets.dart';
import '../../data/members_repository.dart';
import '../../domain/member.dart';

class MembersTabScreen extends StatefulWidget {
  const MembersTabScreen({
    required this.householdId,
    required this.repository,
    this.session,
    super.key,
  });

  final String householdId;
  final MembersRepository repository;
  final AuthSession? session;

  @override
  State<MembersTabScreen> createState() => _MembersTabScreenState();
}

class _MembersTabScreenState extends State<MembersTabScreen> {
  late Future<List<Member>> _members;

  @override
  void initState() {
    super.initState();
    _members = _loadMembers();
  }

  Future<List<Member>> _loadMembers() {
    final AuthSession? session = widget.session;
    if (session == null) {
      return Future<List<Member>>.value(const <Member>[]);
    }
    return widget.repository.getMembers(
      session: session,
      householdId: widget.householdId,
    );
  }

  void _reload() {
    setState(() {
      _members = _loadMembers();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: FutureBuilder<List<Member>>(
        future: _members,
        builder: (BuildContext context, AsyncSnapshot<List<Member>> snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView(label: 'Loading members...');
          }
          if (snapshot.hasError) {
            return ErrorView(
              title: 'Could not load members',
              message: snapshot.error.toString(),
              onRetry: _reload,
            );
          }
          final List<Member> members = snapshot.data ?? const <Member>[];
          if (members.isEmpty) {
            return EmptyView(
              title: 'Invite the rest of your household',
              message: 'Members you invite will appear here.',
              action: FilledButton.icon(
                onPressed: _showInviteSheet,
                icon: const Icon(Icons.person_add_outlined),
                label: const Text('Invite member'),
              ),
            );
          }
          return ListView.separated(
            padding: const EdgeInsets.all(AppSpacing.lg),
            itemCount: members.length + 1,
            separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.md),
            itemBuilder: (BuildContext context, int index) {
              if (index == 0) {
                return AppCard(
                  tonal: true,
                  child: Text(
                    '${members.length} member${members.length == 1 ? '' : 's'} in this household',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                );
              }
              final Member member = members[index - 1];
              return AppCard(
                child: Row(
                  children: <Widget>[
                    EntityAvatar(id: member.id, name: member.name),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: <Widget>[
                          Text(member.name),
                          Text(
                            member.email,
                            style: Theme.of(context).textTheme.bodySmall,
                          ),
                        ],
                      ),
                    ),
                    RoleBadge(role: member.role),
                    PopupMenuButton<String>(
                      tooltip: 'Member actions',
                      itemBuilder: (BuildContext context) =>
                          const <PopupMenuEntry<String>>[
                        PopupMenuItem<String>(
                          value: 'promote',
                          child: Text('Promote'),
                        ),
                        PopupMenuItem<String>(
                          value: 'remove',
                          child: Text('Remove'),
                        ),
                      ],
                    ),
                  ],
                ),
              );
            },
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _showInviteSheet,
        icon: const Icon(Icons.person_add_outlined),
        label: const Text('Invite'),
      ),
    );
  }

  void _showInviteSheet() {
    final TextEditingController controller = TextEditingController();
    MemberRole role = MemberRole.member;
    showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      isScrollControlled: true,
      builder: (BuildContext context) {
        return StatefulBuilder(
          builder: (BuildContext context, StateSetter setSheetState) {
            return Padding(
              padding: EdgeInsets.only(
                left: AppSpacing.lg,
                right: AppSpacing.lg,
                bottom:
                    MediaQuery.viewInsetsOf(context).bottom + AppSpacing.lg,
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  Text('Invite member',
                      style: Theme.of(context).textTheme.titleLarge),
                  const SizedBox(height: AppSpacing.lg),
                  TextField(
                    controller: controller,
                    keyboardType: TextInputType.emailAddress,
                    decoration: const InputDecoration(labelText: 'Email'),
                  ),
                  const SizedBox(height: AppSpacing.md),
                  SegmentedButton<MemberRole>(
                    segments: const <ButtonSegment<MemberRole>>[
                      ButtonSegment<MemberRole>(
                        value: MemberRole.member,
                        label: Text('Member'),
                      ),
                      ButtonSegment<MemberRole>(
                        value: MemberRole.admin,
                        label: Text('Admin'),
                      ),
                    ],
                    selected: <MemberRole>{role},
                    onSelectionChanged: (Set<MemberRole> value) {
                      setSheetState(() => role = value.first);
                    },
                  ),
                  const SizedBox(height: AppSpacing.lg),
                  FilledButton(
                    onPressed: () async {
                      await widget.repository.invite(
                        session: widget.session!,
                        householdId: widget.householdId,
                        email: controller.text.trim(),
                        role: role,
                      );
                      _reload();
                      if (context.mounted) {
                        Navigator.of(context).pop();
                      }
                    },
                    child: const Text('Send invite'),
                  ),
                ],
              ),
            );
          },
        );
      },
    ).whenComplete(controller.dispose);
  }
}
