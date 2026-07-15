import '../domain/household_member.dart';
import '../domain/pending_invitation.dart';

class HouseholdMemberDto {
  const HouseholdMemberDto({
    required this.id,
    required this.displayName,
    required this.role,
    required this.archived,
  });

  factory HouseholdMemberDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final displayName = json['displayName'];
    final role = json['role'];
    final archived = json['archived'];
    if (id is! String ||
        id.isEmpty ||
        displayName is! String ||
        displayName.isEmpty ||
        role is! String ||
        archived is! bool) {
      throw const FormatException('Invalid member response.');
    }
    return HouseholdMemberDto(
      id: id,
      displayName: displayName,
      role: _roleFromJson(role),
      archived: archived,
    );
  }

  final String id;
  final String displayName;
  final HouseholdMemberRole role;
  final bool archived;

  HouseholdMember toDomain() => HouseholdMember(
    id: id,
    displayName: displayName,
    role: role,
    archived: archived,
  );
}

class PendingInvitationDto {
  const PendingInvitationDto({
    required this.id,
    required this.displayName,
    required this.email,
    required this.role,
  });

  factory PendingInvitationDto.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final displayName = json['displayName'];
    final email = json['email'];
    final role = json['role'];
    if (id is! String ||
        id.isEmpty ||
        displayName is! String ||
        displayName.isEmpty ||
        email is! String ||
        email.isEmpty ||
        role is! String) {
      throw const FormatException('Invalid invitation response.');
    }
    return PendingInvitationDto(
      id: id,
      displayName: displayName,
      email: email,
      role: _roleFromJson(role),
    );
  }

  final String id;
  final String displayName;
  final String email;
  final HouseholdMemberRole role;

  PendingInvitation toDomain() => PendingInvitation(
    id: id,
    displayName: displayName,
    email: email,
    role: role,
  );
}

HouseholdMemberRole _roleFromJson(String value) => switch (value) {
  'owner' => HouseholdMemberRole.owner,
  'admin' => HouseholdMemberRole.admin,
  'member' => HouseholdMemberRole.member,
  _ => throw const FormatException('Invalid member role.'),
};
