import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item.dart';

class ItemListViewModel extends ChangeNotifier {
  ItemListViewModel({
    required String householdId,
    required String householdName,
    required String spaceId,
    required ItemsRepository repository,
    AuthSession? session,
  }) : _householdId = householdId,
       _householdName = householdName,
       _spaceId = spaceId,
       _repository = repository,
       _session = session {
    load();
  }

  String _householdId;
  String _householdName;
  String _spaceId;
  ItemsRepository _repository;
  AuthSession? _session;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  List<Item> _items = const <Item>[];
  ConsumableState? _stateFilter;
  String _query = '';

  String get householdId => _householdId;
  String get householdName => _householdName;
  String get spaceId => _spaceId;
  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  List<Item> get allItems => _items;
  ConsumableState? get stateFilter => _stateFilter;
  String get query => _query;

  List<Item> get items {
    final String normalizedQuery = _query.toLowerCase();
    return _items
        .where(
          (Item item) =>
              item.name.toLowerCase().contains(normalizedQuery) &&
              (_stateFilter == null || item.dominantState == _stateFilter),
        )
        .toList(growable: false);
  }

  void updateDependencies({
    required String householdId,
    required String householdName,
    required String spaceId,
    required ItemsRepository repository,
    AuthSession? session,
  }) {
    final bool shouldReload =
        _householdId != householdId ||
        _spaceId != spaceId ||
        _repository != repository ||
        _session != session;

    _householdId = householdId;
    _householdName = householdName;
    _spaceId = spaceId;
    _repository = repository;
    _session = session;

    if (shouldReload) {
      load();
    } else {
      notifyListeners();
    }
  }

  void updateQuery(String value) {
    if (_query == value) {
      return;
    }
    _query = value;
    notifyListeners();
  }

  void updateStateFilter(ConsumableState? value) {
    if (_stateFilter == value) {
      return;
    }
    _stateFilter = value;
    notifyListeners();
  }

  Future<void> load() async {
    _isLoading = true;
    _error = null;
    _stackTrace = null;
    notifyListeners();

    final AuthSession? session = _session;
    if (session == null) {
      _items = const <Item>[];
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      _items = await _repository.getItems(
        session: session,
        householdId: _householdId,
        spaceId: _spaceId,
      );
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> addItem({required String name, String? barcode}) async {
    final AuthSession? session = _session;
    if (session == null) {
      return;
    }

    await _repository.addItem(
      session: session,
      householdId: _householdId,
      spaceId: _spaceId,
      name: name,
      barcode: barcode,
    );
    await load();
  }
}
