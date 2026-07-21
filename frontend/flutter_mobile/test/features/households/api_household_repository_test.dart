import 'dart:convert';

import 'package:domu_mobile/core/api/api_client.dart';
import 'package:domu_mobile/features/households/data/api_household_repository.dart';
import 'package:domu_mobile/features/households/domain/household_repository.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  const household = <String, Object?>{
    'id': '0a97d4b3-7c4e-49a8-b096-5c987bdef5da',
    'name': 'Home',
    'subscriptionPlan': 'free',
    'subscriptionStatus': 'active',
    'subscriptionCurrentPeriodEndsAt': null,
    'subscriptionCancelledAt': null,
  };

  test('lists households with the authenticated API request', () async {
    late http.Request request;
    final repository = ApiHouseholdRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((value) async {
          request = value;
          return http.Response(
            jsonEncode({
              'data': [household],
            }),
            200,
          );
        }),
      ),
    );

    final households = await repository.getHouseholds();

    expect(households.single.name, 'Home');
    expect(request.method, 'GET');
    expect(
      request.url.toString(),
      'https://api.example.test/api/v1/households',
    );
    expect(request.headers['authorization'], 'Bearer token');
  });

  test('serializes create requests and maps failures to safe errors', () async {
    late http.Request request;
    final repository = ApiHouseholdRepository(
      ApiClient(
        baseUrl: 'https://api.example.test/',
        accessToken: () async => 'token',
        httpClient: MockClient((value) async {
          request = value;
          return http.Response(jsonEncode({'data': household}), 201);
        }),
      ),
    );

    await repository.createHousehold(name: 'Home', ownerDisplayName: 'Ada');

    expect(request.method, 'POST');
    expect(jsonDecode(request.body), {
      'name': 'Home',
      'ownerDisplayName': 'Ada',
    });

    final failingRepository = ApiHouseholdRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((_) async => http.Response('', 403)),
      ),
    );
    expect(
      failingRepository.deleteHousehold('id'),
      throwsA(isA<HouseholdRepositoryException>()),
    );
  });

  test('gets and maps expired and upcoming household entries', () async {
    late http.Request request;
    final repository = ApiHouseholdRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((value) async {
          request = value;
          return http.Response(
            jsonEncode({
              'data': {
                'evaluatedAtUtc': '2026-07-21T00:00:00Z',
                'expired': [_expiration('2026-07-20T00:00:00Z')],
                'upcoming': [_expiration('2026-07-25T00:00:00Z')],
              },
            }),
            200,
          );
        }),
      ),
    );

    final result = await repository.getHouseholdExpirations(
      householdId: 'household-id',
      upcomingUntil: DateTime.utc(2026, 8, 20),
    );

    expect(result.expired.single.itemName, 'Milk');
    expect(result.upcoming.single.spaceName, 'Fridge');
    expect(request.method, 'GET');
    expect(request.url.path, '/api/v1/households/household-id/expirations');
    expect(
      request.url.queryParameters['upcomingUntilUtc'],
      '2026-08-20T00:00:00.000Z',
    );
  });
}

Map<String, Object?> _expiration(String expirationDate) => {
  'entryId': 'entry-id',
  'count': 2,
  'originalAmountPerUnit': 1.0,
  'currentAmountPerUnit': null,
  'unit': 'liter',
  'state': 'opened',
  'acquisitionDate': null,
  'expirationDate': expirationDate,
  'itemId': 'item-id',
  'itemName': 'Milk',
  'spaceId': 'space-id',
  'spaceName': 'Fridge',
};
