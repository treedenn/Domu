import 'dart:convert';

import 'package:domu_mobile/core/api/api_client.dart';
import 'package:domu_mobile/features/shopping_lists/data/api_shopping_lists_repository.dart';
import 'package:domu_mobile/features/shopping_lists/domain/shopping_list.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  test('maps shopping-list item count, amount, and unit API fields', () async {
    late http.Request request;
    final repository = ApiShoppingListsRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((value) async {
          request = value;
          return http.Response(
            jsonEncode({
              'data': {
                'id': 'item-1',
                'shoppingListId': 'list-1',
                'name': 'Rice',
                'note': null,
                'count': 2,
                'amountPerUnit': 500,
                'unit': 'gram',
                'spaceId': null,
                'itemId': null,
                'checked': false,
                'sortOrder': 1.5,
              },
            }),
            200,
          );
        }),
      ),
    );

    final item = await repository.updateItem(
      householdId: 'home',
      shoppingListId: 'list-1',
      item: const ShoppingListItem(
        id: 'item-1',
        shoppingListId: 'list-1',
        name: 'Rice',
        note: null,
        count: 2,
        amountPerUnit: 500,
        unit: ShoppingListItemUnit.gram,
        spaceId: null,
        itemId: null,
        checked: false,
        sortOrder: 1.5,
      ),
      name: 'Rice',
    );

    expect(item.count, 2);
    expect(item.amountPerUnit, 500);
    expect(item.unit, ShoppingListItemUnit.gram);
    expect(item.sortOrder, 1.5);
    expect(jsonDecode(request.body), {
      'name': 'Rice',
      'note': null,
      'count': 2,
      'amountPerUnit': 500,
      'unit': 'gram',
      'spaceId': null,
      'itemId': null,
      'sortOrder': 1.5,
    });
  });
}
