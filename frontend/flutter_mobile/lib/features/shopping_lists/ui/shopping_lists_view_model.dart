import 'package:flutter/foundation.dart';
import '../domain/shopping_list.dart';
import '../domain/shopping_lists_repository.dart';

class ShoppingListsViewModel extends ChangeNotifier {
  ShoppingListsViewModel(this._repository);
  final ShoppingListsRepository _repository;
  String? _householdId;
  List<ShoppingList> _lists = const [];
  String? _errorMessage;
  String? _message;
  bool _isLoading = false;
  bool _isRefreshing = false;
  bool _isMutating = false;
  List<ShoppingList> get lists => List.unmodifiable(_lists);
  String? get errorMessage => _errorMessage;
  String? get message => _message;
  bool get isLoading => _isLoading;
  bool get isRefreshing => _isRefreshing;
  bool get isMutating => _isMutating;
  bool get isEmpty => !_isLoading && _lists.isEmpty && _errorMessage == null;
  void clearMessage() {
    _message = null;
    notifyListeners();
  }

  Future<void> load(String householdId) =>
      _load(householdId, initial: _householdId != householdId);
  Future<void> refresh() => _householdId == null
      ? Future.value()
      : _load(_householdId!, initial: false);
  Future<void> _load(String id, {required bool initial}) async {
    if (_isLoading || _isRefreshing) return;
    _householdId = id;
    initial ? _isLoading = true : _isRefreshing = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _lists = await _repository.getShoppingLists(id);
    } on ShoppingListsRepositoryException catch (e) {
      _errorMessage = e.message;
    } catch (_) {
      _errorMessage = 'Unable to load shopping lists. Please try again.';
    } finally {
      _isLoading = false;
      _isRefreshing = false;
      notifyListeners();
    }
  }

  Future<bool> create(String name) => _mutate(
    'Shopping list created.',
    (id) => _repository.createShoppingList(householdId: id, name: name),
  );
  Future<bool> rename(ShoppingList list, String name) => _mutate(
    'Shopping list renamed.',
    (id) =>
        _repository.renameShoppingList(householdId: id, list: list, name: name),
  );
  Future<bool> archive(ShoppingList list) => _mutate(
    'Shopping list deleted.',
    (id) => _repository.archiveShoppingList(householdId: id, list: list),
  );
  Future<bool> _mutate(
    String success,
    Future<Object?> Function(String) action,
  ) async {
    final id = _householdId;
    if (id == null || _isMutating) return false;
    _isMutating = true;
    _message = null;
    notifyListeners();
    try {
      await action(id);
      _lists = await _repository.getShoppingLists(id);
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
}
