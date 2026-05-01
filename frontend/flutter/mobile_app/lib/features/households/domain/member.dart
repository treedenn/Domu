class Member {
  const Member({
    required this.id,
    required this.name,
    required this.email,
    required this.role,
  });

  final String id;
  final String name;
  final String email;
  final MemberRole role;

  factory Member.fromJson(Map<String, Object?> json) {
    final String userId = json['userId']?.toString() ?? '';
    final String displayId = userId.isEmpty ? json['id'].toString() : userId;
    return Member(
      id: json['id'].toString(),
      name: 'Member ${displayId.length > 8 ? displayId.substring(0, 8) : displayId}',
      email: displayId,
      role: MemberRoleX.fromJson(json['role']),
    );
  }
}

enum MemberRole { owner, admin, member }

extension MemberRoleX on MemberRole {
  static MemberRole fromJson(Object? value) {
    return switch (value) {
      String text when text.toLowerCase() == 'owner' => MemberRole.owner,
      String text when text.toLowerCase() == 'admin' => MemberRole.admin,
      String text when text.toLowerCase() == 'member' => MemberRole.member,
      0 => MemberRole.owner,
      1 => MemberRole.admin,
      2 => MemberRole.member,
      _ => MemberRole.member,
    };
  }

  int toJson() {
    return switch (this) {
      MemberRole.owner => 0,
      MemberRole.admin => 1,
      MemberRole.member => 2,
    };
  }
}
