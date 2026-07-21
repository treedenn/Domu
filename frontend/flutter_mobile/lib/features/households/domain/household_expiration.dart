import '../../spaces/domain/space.dart';

class HouseholdExpirations {
  const HouseholdExpirations({
    required this.evaluatedAt,
    required this.expired,
    required this.upcoming,
  });

  final DateTime evaluatedAt;
  final List<HouseholdExpiration> expired;
  final List<HouseholdExpiration> upcoming;
}

class HouseholdExpiration {
  const HouseholdExpiration({
    required this.id,
    required this.count,
    required this.unit,
    required this.state,
    required this.expirationDate,
    required this.itemId,
    required this.itemName,
    required this.spaceId,
    required this.spaceName,
    this.originalAmountPerUnit,
    this.currentAmountPerUnit,
    this.acquisitionDate,
  });

  final String id;
  final int count;
  final num? originalAmountPerUnit;
  final num? currentAmountPerUnit;
  final ItemUnit unit;
  final ConsumableState state;
  final DateTime? acquisitionDate;
  final DateTime expirationDate;
  final String itemId;
  final String itemName;
  final String spaceId;
  final String spaceName;
}
