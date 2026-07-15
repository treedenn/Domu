enum HouseholdMemberRole {
  owner,
  admin,
  member;

  String get label => switch (this) {
    HouseholdMemberRole.owner => 'Owner',
    HouseholdMemberRole.admin => 'Admin',
    HouseholdMemberRole.member => 'Member',
  };
}

class HouseholdMember {
  const HouseholdMember({
    required this.id,
    required this.displayName,
    required this.role,
    required this.archived,
  });

  final String id;
  final String displayName;
  final HouseholdMemberRole role;
  final bool archived;
}
