import 'dart:convert';

import 'package:domu_mobile/core/api/api_client.dart';
import 'package:domu_mobile/features/spaces/data/api_spaces_repository.dart';
import 'package:domu_mobile/features/spaces/domain/space.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  test('lists direct spaces with hierarchy counts and paging flags', () async {
    late http.Request request;
    final repository = ApiSpacesRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((value) async {
          request = value;
          return http.Response(
            jsonEncode({
              'data': {
                'spaces': [
                  {
                    'id': 'pantry',
                    'householdId': 'home',
                    'parentId': null,
                    'name': 'Pantry',
                    'description': null,
                    'items': {'count': 2},
                    'childSpaces': {'count': 1},
                  },
                ],
                'pageNumber': 1,
                'pageSize': 20,
                'totalCount': 21,
              },
            }),
            200,
          );
        }),
      ),
    );
    final page = await repository.getSpaces(householdId: 'home');
    expect(request.url.path, '/api/v1/households/home/spaces');
    expect(request.url.queryParameters['includeItemCount'], 'true');
    expect(request.url.queryParameters['includeChildSpaceCount'], 'true');
    expect(page.spaces.single.itemCount, 2);
    expect(page.spaces.single.childSpaceCount, 1);
    expect(page.hasMore, isTrue);
  });

  test(
    'serializes inventory entry units and states using API enum names',
    () async {
      late http.Request request;
      final repository = ApiSpacesRepository(
        ApiClient(
          baseUrl: 'https://api.example.test',
          accessToken: () async => 'token',
          httpClient: MockClient((value) async {
            request = value;
            return http.Response(
              jsonEncode({
                'data': {
                  'id': 'rice',
                  'spaceId': 'pantry',
                  'name': 'Rice',
                  'category': null,
                  'barcode': null,
                  'totalQuantity': 1,
                  'entries': [],
                },
              }),
              201,
            );
          }),
        ),
      );
      await repository.createItem(
        householdId: 'home',
        spaceId: 'pantry',
        name: 'Rice',
        entries: const [
          ItemEntry(
            originalQuantity: 1,
            currentQuantity: 1,
            unit: ItemUnit.kilogram,
            state: ConsumableState.unopened,
          ),
        ],
      );
      final body = jsonDecode(request.body) as Map<String, dynamic>;
      expect(body['entries'][0]['originalQuantity'], 1);
      expect(body['entries'][0], isNot(contains('initialQuantity')));
      expect(body['entries'][0]['unit'], 'kilogram');
      expect(body['entries'][0]['state'], 'unopened');
      expect(body['entries'][0], isNot(contains('containerType')));
    },
  );
}
