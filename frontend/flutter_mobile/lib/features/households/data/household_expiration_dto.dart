import '../../spaces/domain/space.dart';
import '../domain/household_expiration.dart';

class HouseholdExpirationDto {
  const HouseholdExpirationDto({
    required this.id,
    required this.count,
    required this.originalAmountPerUnit,
    required this.currentAmountPerUnit,
    required this.unit,
    required this.state,
    required this.acquisitionDate,
    required this.expirationDate,
    required this.itemId,
    required this.itemName,
    required this.spaceId,
    required this.spaceName,
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

  factory HouseholdExpirationDto.fromJson(Map<String, dynamic> json) =>
      HouseholdExpirationDto(
        id: json['entryId'] as String,
        count: json['count'] as int,
        originalAmountPerUnit: json['originalAmountPerUnit'] as num?,
        currentAmountPerUnit: json['currentAmountPerUnit'] as num?,
        unit: ItemUnit.values.byName(json['unit'] as String),
        state: ConsumableState.values.byName(json['state'] as String),
        acquisitionDate: _date(json['acquisitionDate'] as String?),
        expirationDate: DateTime.parse(json['expirationDate'] as String),
        itemId: json['itemId'] as String,
        itemName: json['itemName'] as String,
        spaceId: json['spaceId'] as String,
        spaceName: json['spaceName'] as String,
      );

  HouseholdExpiration toDomain() => HouseholdExpiration(
    id: id,
    count: count,
    originalAmountPerUnit: originalAmountPerUnit,
    currentAmountPerUnit: currentAmountPerUnit,
    unit: unit,
    state: state,
    acquisitionDate: acquisitionDate,
    expirationDate: expirationDate,
    itemId: itemId,
    itemName: itemName,
    spaceId: spaceId,
    spaceName: spaceName,
  );

  static DateTime? _date(String? value) =>
      value == null ? null : DateTime.parse(value);
}
