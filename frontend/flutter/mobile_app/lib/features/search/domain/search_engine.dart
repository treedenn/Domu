import '../../items/domain/item.dart';
import '../../spaces/domain/space.dart';
import 'search_query.dart';

class SearchResults {
  const SearchResults({required this.spaces, required this.items});

  final List<Space> spaces;
  final List<Item> items;
}

class SearchEngine {
  SearchResults search({
    required SearchQuery query,
    required List<Space> spaces,
    required List<Item> items,
  }) {
    final String text = query.text.trim().toLowerCase();
    final DateTime now = DateTime.now();

    final List<Space> spaceHits = text.isEmpty
        ? const <Space>[]
        : spaces
            .where((Space space) => space.name.toLowerCase().contains(text))
            .toList(growable: false);
    final List<Item> itemHits = items.where((Item item) {
      final bool textMatches = text.isEmpty ||
          item.name.toLowerCase().contains(text) ||
          item.barcode == query.text.trim();
      final bool expiryMatches = query.expiringWithinDays == null ||
          (item.earliestExpiresAt != null &&
              item.earliestExpiresAt!.difference(now).inDays <=
                  query.expiringWithinDays!);
      return textMatches && expiryMatches;
    }).toList(growable: false)
      ..sort((Item a, Item b) => _rank(query.text, b).compareTo(_rank(query.text, a)));

    return SearchResults(spaces: spaceHits, items: itemHits);
  }

  int _rank(String text, Item item) {
    final String normalized = text.trim().toLowerCase();
    if (normalized.isNotEmpty && item.barcode == text.trim()) {
      return 3;
    }
    if (item.name.toLowerCase().startsWith(normalized)) {
      return 2;
    }
    if (item.name.toLowerCase().contains(normalized)) {
      return 1;
    }
    return 0;
  }
}
