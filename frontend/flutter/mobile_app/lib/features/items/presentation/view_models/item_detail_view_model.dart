import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/items_repository.dart';
import '../../domain/item.dart';
import '../../domain/item_entry.dart';

class ItemDetailViewModel extends ChangeNotifier {
  ItemDetailViewModel({
    required String householdId,
    required String spaceId,
    required String itemId,
    required ItemsRepository repository,
    AuthSession? session,
  }) : _householdId = householdId,
       _spaceId = spaceId,
       _itemId = itemId,
       _repository = repository,
       _session = session {
    load();
  }

  String _householdId;
  String _spaceId;
  String _itemId;
  ItemsRepository _repository;
  AuthSession? _session;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  Item? _item;
  List<ItemEntry> _entries = const <ItemEntry>[];

  String get householdId => _householdId;
  String get spaceId => _spaceId;
  String get itemId => _itemId;
  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  Item? get item => _item;
  List<ItemEntry> get entries => _entries;

  void updateDependencies({
    required String householdId,
    required String spaceId,
    required String itemId,
    required ItemsRepository repository,
    AuthSession? session,
  }) {
    final bool shouldReload =
        _householdId != householdId ||
        _spaceId != spaceId ||
        _itemId != itemId ||
        _repository != repository ||
        _session != session;

    _householdId = householdId;
    _spaceId = spaceId;
    _itemId = itemId;
    _repository = repository;
    _session = session;

    if (shouldReload) {
      load();
    } else {
      notifyListeners();
    }
  }

  Future<void> load() async {
    _isLoading = true;
    _error = null;
    _stackTrace = null;
    notifyListeners();

    final AuthSession? session = _session;
    if (session == null) {
      _item = null;
      _entries = const <ItemEntry>[];
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      _item = await _repository.getItem(
        session: session,
        householdId: _householdId,
        spaceId: _spaceId,
        itemId: _itemId,
      );
      _entries = await _repository.getEntries(
        session: session,
        householdId: _householdId,
        spaceId: _spaceId,
        itemId: _itemId,
      );
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
