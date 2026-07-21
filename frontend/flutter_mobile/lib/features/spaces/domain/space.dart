enum ItemUnit { unspecified, piece, milliliter, liter, gram, kilogram }

enum ConsumableState { unspecified, unopened, opened }

class Space {
  const Space({
    required this.id,
    required this.householdId,
    required this.name,
    this.parentId,
    this.description,
    this.itemCount = 0,
    this.childSpaceCount = 0,
  });

  final String id;
  final String householdId;
  final String name;
  final String? parentId;
  final String? description;
  final int itemCount;
  final int childSpaceCount;
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
  bool get hasMore => pageNumber * pageSize < totalCount;
}

class SpaceItem {
  const SpaceItem({
    required this.id,
    required this.spaceId,
    required this.name,
    required this.totalCount,
    required this.entries,
    this.category,
    this.barcode,
  });

  final String id;
  final String spaceId;
  final String name;
  final String? category;
  final String? barcode;
  final int totalCount;
  final List<ItemEntry> entries;
}

class ItemEntry {
  const ItemEntry({
    this.id,
    required this.count,
    this.originalAmountPerUnit,
    this.currentAmountPerUnit,
    required this.unit,
    required this.state,
    this.acquisitionDate,
    this.expirationDate,
  });

  final String? id;
  final int count;
  final num? originalAmountPerUnit;
  final num? currentAmountPerUnit;
  final ItemUnit unit;
  final ConsumableState state;
  final DateTime? acquisitionDate;
  final DateTime? expirationDate;
}
