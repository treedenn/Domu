import 'package:domu_mobile/features/members/domain/household_member.dart';
import 'package:domu_mobile/features/members/domain/members_repository.dart';
import 'package:domu_mobile/features/members/domain/members_result.dart';
import 'package:domu_mobile/features/members/domain/pending_invitation.dart';
import 'package:domu_mobile/features/members/ui/members_view_model.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test(
    'loads members in role and display-name order with pending invitations',
    () async {
      final repository = _FakeMembersRepository();
      final viewModel = MembersViewModel(repository);

      await viewModel.load('home');

      expect(viewModel.members.map((member) => member.displayName), [
        'Zoe',
        'Ada',
        'Grace',
      ]);
      expect(viewModel.pendingInvitations.single.email, 'lin@example.test');
      expect(viewModel.canManageMembers, isTrue);
    },
  );

  test('inviting and archiving reloads members and reports success', () async {
    final repository = _FakeMembersRepository();
    final viewModel = MembersViewModel(repository);
    await viewModel.load('home');

    expect(
      await viewModel.invite(
        displayName: 'Lin',
        email: 'lin@example.test',
        role: HouseholdMemberRole.member,
      ),
      isTrue,
    );
    expect(viewModel.message, 'Invitation sent.');
    expect(repository.invited, isTrue);

    expect(await viewModel.archive(viewModel.members[1]), isTrue);
    expect(repository.archivedMemberId, 'admin');
    expect(viewModel.message, 'Ada removed.');
  });
}

class _FakeMembersRepository implements MembersRepository {
  bool invited = false;
  String? archivedMemberId;

  @override
  Future<void> archiveMember({
    required String householdId,
    required HouseholdMember member,
  }) async {
    archivedMemberId = member.id;
  }

  @override
  Future<PendingInvitation> createInvitation({
    required String householdId,
    required String displayName,
    required String email,
    required HouseholdMemberRole role,
  }) async {
    invited = true;
    return PendingInvitation(
      id: 'new',
      displayName: displayName,
      email: email,
      role: role,
    );
  }

  @override
  Future<MembersResult> getMembers(String householdId) async =>
      const MembersResult(
        canManageMembers: true,
        members: [
          HouseholdMember(
            id: 'admin',
            displayName: 'Ada',
            role: HouseholdMemberRole.admin,
            archived: false,
          ),
          HouseholdMember(
            id: 'owner',
            displayName: 'Zoe',
            role: HouseholdMemberRole.owner,
            archived: false,
          ),
          HouseholdMember(
            id: 'member',
            displayName: 'Grace',
            role: HouseholdMemberRole.member,
            archived: false,
          ),
        ],
      );

  @override
  Future<List<PendingInvitation>> getPendingInvitations(
    String householdId,
  ) async => const [
    PendingInvitation(
      id: 'pending',
      displayName: 'Lin',
      email: 'lin@example.test',
      role: HouseholdMemberRole.member,
    ),
  ];
}
