import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item_entry.dart';

class EntryEditorViewModel extends ChangeNotifier {
  EntryEditorViewModel({
    required String householdId,
    required String spaceId,
    required String itemId,
    required ItemsRepository repository,
    AuthSession? session,
    String? entryId,
  }) : _householdId = householdId,
       _spaceId = spaceId,
       _itemId = itemId,
       _repository = repository,
       _session = session,
       _entryId = entryId;

  String _householdId;
  String _spaceId;
  String _itemId;
  ItemsRepository _repository;
  AuthSession? _session;
  String? _entryId;
  int _quantity = 1;
  DateTime _acquiredAt = DateTime.now();
  DateTime? _expiresAt;
  ConsumableState _state = ConsumableState.unknown;
  String? _error;
  bool _isSaving = false;

  int get quantity => _quantity;
  DateTime get acquiredAt => _acquiredAt;
  DateTime? get expiresAt => _expiresAt;
  ConsumableState get state => _state;
  String? get error => _error;
  bool get isSaving => _isSaving;

  void updateDependencies({
    required String householdId,
    required String spaceId,
    required String itemId,
    required ItemsRepository repository,
    AuthSession? session,
    String? entryId,
  }) {
    _householdId = householdId;
    _spaceId = spaceId;
    _itemId = itemId;
    _repository = repository;
    _session = session;
    _entryId = entryId;
    notifyListeners();
  }

  void updateQuantity(int value) {
    _quantity = value;
    _error = null;
    notifyListeners();
  }

  void updateAcquiredAt(DateTime value) {
    _acquiredAt = value;
    _error = null;
    notifyListeners();
  }

  void updateExpiresAt(DateTime? value) {
    _expiresAt = value;
    _error = null;
    notifyListeners();
  }

  void updateState(ConsumableState value) {
    _state = value;
    _error = null;
    notifyListeners();
  }

  Future<bool> save() async {
    if (_expiresAt != null && _expiresAt!.isBefore(_acquiredAt)) {
      _error = 'Expiration must be after acquired date.';
      notifyListeners();
      return false;
    }

    final AuthSession? session = _session;
    if (session == null) {
      _error = 'You need to sign in before saving.';
      notifyListeners();
      return false;
    }

    _isSaving = true;
    _error = null;
    notifyListeners();

    try {
      await _repository.saveEntry(
        session: session,
        householdId: _householdId,
        spaceId: _spaceId,
        entry: ItemEntry(
          id: _entryId ?? '',
          itemId: _itemId,
          quantity: _quantity,
          acquiredAt: _acquiredAt,
          expiresAt: _expiresAt,
          state: _state,
        ),
      );
      return true;
    } catch (error) {
      _error = error.toString();
      return false;
    } finally {
      _isSaving = false;
      notifyListeners();
    }
  }
}
