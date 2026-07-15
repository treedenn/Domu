import 'household_member.dart';
import 'members_result.dart';
import 'pending_invitation.dart';

abstract interface class MembersRepository {
  Future<MembersResult> getMembers(String householdId);
  Future<List<PendingInvitation>> getPendingInvitations(String householdId);
  Future<PendingInvitation> createInvitation({
    required String householdId,
    required String displayName,
    required String email,
    required HouseholdMemberRole role,
  });
  Future<void> archiveMember({
    required String householdId,
    required HouseholdMember member,
  });
}

class MembersRepositoryException implements Exception {
  const MembersRepositoryException(this.message);

  final String message;

  @override
  String toString() => message;
}
