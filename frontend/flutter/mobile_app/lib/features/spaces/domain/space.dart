class Space {
  const Space({
    required this.id,
    required this.householdId,
    required this.parentId,
    required this.name,
    required this.description,
    required this.itemCount,
    required this.childSpaceCount,
  });

  final String id;
  final String householdId;
  final String? parentId;
  final String name;
  final String? description;
  final int itemCount;
  final int childSpaceCount;

  factory Space.fromJson(Map<String, Object?> json) {
    return Space(
      id: json['id'] as String,
      householdId: json['householdId'] as String,
      parentId: json['parentId'] as String?,
      name: json['name'] as String,
      description: json['description'] as String?,
      itemCount: _collectionCount(json['items']),
      childSpaceCount: _collectionCount(json['childSpaces']),
    );
  }

  static int _collectionCount(Object? json) {
    if (json is Map<String, Object?>) {
      return json['count'] as int? ?? 0;
    }

    return 0;
  }
}

class SpacePage {
  const SpacePage({
    required this.spaces,
    required this.pageNumber,
    required this.pageSize,
    required this.totalCount,
  });

  final List<Space> spaces;
  final int pageNumber;
  final int pageSize;
  final int totalCount;

  factory SpacePage.fromJson(Map<String, Object?> json) {
    final Object? spacesJson = json['spaces'];
    return SpacePage(
      spaces: spacesJson is List<Object?>
          ? spacesJson
              .whereType<Map<String, Object?>>()
              .map(Space.fromJson)
              .toList(growable: false)
          : const <Space>[],
      pageNumber: json['pageNumber'] as int? ?? 1,
      pageSize: json['pageSize'] as int? ?? 20,
      totalCount: json['totalCount'] as int? ?? 0,
    );
  }
}
