import 'household_member.dart';

class MembersResult {
  const MembersResult({required this.members, required this.canManageMembers});

  final List<HouseholdMember> members;
  final bool canManageMembers;
}
