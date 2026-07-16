import 'package:flutter/foundation.dart';

import '../domain/space.dart';
import '../domain/spaces_repository.dart';

class SpacesViewModel extends ChangeNotifier {
  SpacesViewModel(this._repository);
  final SpacesRepository _repository;
  String? _householdId;
  String? _parentId;
  List<Space> _spaces = const [];
  List<SpaceItem> _items = const [];
  Space? _currentSpace;
  String? _errorMessage;
  String? _message;
  bool _isLoading = false;
  bool _isMutating = false;
  bool _hasMore = false;
  int _nextPage = 1;
  List<Space> get spaces => List.unmodifiable(_spaces);
  List<SpaceItem> get items => List.unmodifiable(_items);
  Space? get currentSpace => _currentSpace;
  String? get errorMessage => _errorMessage;
  String? get message => _message;
  bool get isLoading => _isLoading;
  bool get isMutating => _isMutating;
  bool get hasMore => _hasMore;
  bool get isEmpty =>
      !_isLoading && _spaces.isEmpty && _items.isEmpty && _errorMessage == null;
  void clearMessage() {
    _message = null;
    notifyListeners();
  }

  Future<void> load(
    String householdId, {
    String? parentId,
    bool refresh = false,
  }) async {
    if (_isLoading) return;
    final changed = _householdId != householdId || _parentId != parentId;
    _householdId = householdId;
    _parentId = parentId;
    _isLoading = true;
    _errorMessage = null;
    if (changed || refresh) {
      _spaces = const [];
      _items = const [];
      _nextPage = 1;
      _hasMore = false;
    }
    notifyListeners();
    try {
      if (parentId != null) {
        _currentSpace = await _repository.getSpace(
          householdId: householdId,
          spaceId: parentId,
        );
        _items = await _repository.getItems(
          householdId: householdId,
          spaceId: parentId,
        );
      } else {
        _currentSpace = null;
        _items = const [];
      }
      final page = await _repository.getSpaces(
        householdId: householdId,
        parentId: parentId,
        pageNumber: 1,
      );
      _spaces = page.spaces;
      _nextPage = 2;
      _hasMore = page.hasMore;
    } on SpacesRepositoryException catch (e) {
      _errorMessage = e.message;
    } catch (_) {
      _errorMessage = 'Unable to load spaces. Please try again.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> loadMore() async {
    final householdId = _householdId;
    if (householdId == null || _isLoading || !_hasMore) return;
    _isLoading = true;
    notifyListeners();
    try {
      final page = await _repository.getSpaces(
        householdId: householdId,
        parentId: _parentId,
        pageNumber: _nextPage,
      );
      _spaces = [..._spaces, ...page.spaces];
      _nextPage++;
      _hasMore = page.hasMore;
    } on SpacesRepositoryException catch (e) {
      _message = e.message;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> createSpace(String name, {String? description}) => _mutate(
    'Space created.',
    (h) => _repository.createSpace(
      householdId: h,
      name: name,
      description: description,
      parentId: _parentId,
    ),
  );
  Future<bool> updateSpace(
    String spaceId,
    String name, {
    String? description,
  }) => _mutate(
    'Space updated.',
    (h) => _repository.updateSpace(
      householdId: h,
      spaceId: spaceId,
      name: name,
      description: description,
    ),
  );
  Future<bool> moveSpace(String spaceId, String? parentId) => _mutate(
    'Space moved.',
    (h) => _repository.moveSpace(
      householdId: h,
      spaceId: spaceId,
      parentId: parentId,
    ),
  );
  Future<bool> deleteSpace(String spaceId) => _mutate(
    'Space deleted.',
    (h) => _repository.deleteSpace(householdId: h, spaceId: spaceId),
  );
  Future<bool> createItem(
    String name, {
    String? category,
    String? barcode,
    List<ItemEntry>? entries,
  }) => _mutate(
    'Item created.',
    (h) => _repository.createItem(
      householdId: h,
      spaceId: _requiredParent,
      name: name,
      category: category,
      barcode: barcode,
      entries: entries,
    ),
  );
  Future<bool> updateItem(
    SpaceItem item,
    String name, {
    String? category,
    String? barcode,
    required List<ItemEntry> entries,
  }) => _mutate('Item updated.', (h) async {
    await _repository.updateItem(
      householdId: h,
      spaceId: _requiredParent,
      itemId: item.id,
      name: name,
      category: category,
      barcode: barcode,
    );
    await _repository.replaceItemEntries(
      householdId: h,
      spaceId: _requiredParent,
      itemId: item.id,
      entries: entries,
    );
  });
  Future<bool> deleteItem(String itemId) => _mutate(
    'Item deleted.',
    (h) => _repository.deleteItem(
      householdId: h,
      spaceId: _requiredParent,
      itemId: itemId,
    ),
  );
  String get _requiredParent =>
      _parentId ?? (throw StateError('Items require a space.'));

  Future<List<Space>> moveDestinations(String movingSpaceId) async {
    final householdId = _householdId;
    if (householdId == null) return const [];
    final all = <Space>[];
    final descendants = <String>{movingSpaceId};
    Future<void> visit(
      String? parentId, {
      bool insideMovingTree = false,
    }) async {
      var pageNumber = 1;
      while (true) {
        final page = await _repository.getSpaces(
          householdId: householdId,
          parentId: parentId,
          pageNumber: pageNumber,
        );
        for (final space in page.spaces) {
          final isDescendant = insideMovingTree || space.id == movingSpaceId;
          if (isDescendant) descendants.add(space.id);
          all.add(space);
          await visit(space.id, insideMovingTree: isDescendant);
        }
        if (!page.hasMore) break;
        pageNumber++;
      }
    }

    await visit(null);
    return all.where((space) => !descendants.contains(space.id)).toList();
  }

  Future<bool> _mutate(
    String success,
    Future<void> Function(String) action,
  ) async {
    final h = _householdId;
    if (h == null || _isMutating) return false;
    _isMutating = true;
    _message = null;
    notifyListeners();
    try {
      await action(h);
      await load(h, parentId: _parentId, refresh: true);
      _message = success;
      return true;
    } on SpacesRepositoryException catch (e) {
      _message = e.message;
      return false;
    } catch (_) {
      _message = 'Unable to complete that request. Please try again.';
      return false;
    } finally {
      _isMutating = false;
      notifyListeners();
    }
  }
}
