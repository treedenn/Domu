import 'package:flutter/foundation.dart';
import '../domain/shopping_list.dart';
import '../domain/shopping_lists_repository.dart';

class ShoppingListDetailViewModel extends ChangeNotifier {
  ShoppingListDetailViewModel(this._repository);
  final ShoppingListsRepository _repository;
  String? _householdId;
  String? _listId;
  List<ShoppingListItem> _items = const [];
  String? _errorMessage;
  String? _message;
  bool _isLoading = false;
  bool _isRefreshing = false;
  bool _isMutating = false;
  List<ShoppingListItem> get items => List.unmodifiable(_items);
  List<ShoppingListItem> get uncheckedItems =>
      _items.where((e) => !e.checked).toList();
  List<ShoppingListItem> get completedItems =>
      _items.where((e) => e.checked).toList();
  String? get errorMessage => _errorMessage;
  String? get message => _message;
  bool get isLoading => _isLoading;
  bool get isMutating => _isMutating;
  bool get isEmpty => !_isLoading && _items.isEmpty && _errorMessage == null;
  bool get hasCompleted => _items.any((e) => e.checked);
  void clearMessage() {
    _message = null;
    notifyListeners();
  }

  Future<void> load(String householdId, String listId) => _load(
    householdId,
    listId,
    initial: _householdId != householdId || _listId != listId,
  );
  Future<void> refresh() => _householdId == null || _listId == null
      ? Future.value()
      : _load(_householdId!, _listId!, initial: false);
  Future<void> _load(
    String householdId,
    String listId, {
    required bool initial,
  }) async {
    if (_isLoading || _isRefreshing) return;
    _householdId = householdId;
    _listId = listId;
    initial ? _isLoading = true : _isRefreshing = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _items = _sorted(
        await _repository.getItems(
          householdId: householdId,
          shoppingListId: listId,
        ),
      );
    } on ShoppingListsRepositoryException catch (e) {
      _errorMessage = e.message;
    } catch (_) {
      _errorMessage = 'Unable to load shopping list items. Please try again.';
    } finally {
      _isLoading = false;
      _isRefreshing = false;
      notifyListeners();
    }
  }

  Future<bool> add(String name, {String? note}) => _mutate(
    'Item added.',
    (h, l) => _repository.createItem(
      householdId: h,
      shoppingListId: l,
      name: name,
      note: note,
    ),
  );
  Future<bool> toggle(ShoppingListItem item) => _mutate(
    item.checked ? 'Item unchecked.' : 'Item checked.',
    (h, l) => _repository.setItemChecked(
      householdId: h,
      shoppingListId: l,
      itemId: item.id,
      checked: !item.checked,
    ),
  );
  Future<bool> update(ShoppingListItem item, String name, {String? note}) =>
      _mutate(
        'Item updated.',
        (h, l) => _repository.updateItem(
          householdId: h,
          shoppingListId: l,
          item: item,
          name: name,
          note: note,
        ),
      );
  Future<bool> delete(ShoppingListItem item) => _mutate(
    'Item deleted.',
    (h, l) => _repository.deleteItem(
      householdId: h,
      shoppingListId: l,
      itemId: item.id,
    ),
  );
  Future<bool> clearCompleted() => _mutate(
    'Completed items cleared.',
    (h, l) => _repository.clearCompleted(householdId: h, shoppingListId: l),
  );
  Future<bool> _mutate(
    String success,
    Future<Object?> Function(String, String) action,
  ) async {
    final h = _householdId;
    final l = _listId;
    if (h == null || l == null || _isMutating) return false;
    _isMutating = true;
    _message = null;
    notifyListeners();
    try {
      await action(h, l);
      _items = _sorted(
        await _repository.getItems(householdId: h, shoppingListId: l),
      );
      _message = success;
      return true;
    } on ShoppingListsRepositoryException catch (e) {
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

  static List<ShoppingListItem> _sorted(List<ShoppingListItem> items) {
    final result = [...items];
    result.sort((a, b) {
      final c = a.checked == b.checked ? 0 : (a.checked ? 1 : -1);
      return c != 0 ? c : a.sortOrder.compareTo(b.sortOrder);
    });
    return result;
  }
}
