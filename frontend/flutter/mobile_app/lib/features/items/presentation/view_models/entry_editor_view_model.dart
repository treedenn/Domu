import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/items_repository.dart';
import '../../domain/consumable_state.dart';
import '../../domain/item_container_type.dart';
import '../../domain/item_entry.dart';
import '../../domain/item_unit.dart';

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
  double _initialQuantity = 1;
  double _currentQuantity = 1;
  ItemUnit _unit = ItemUnit.piece;
  ItemContainerType _containerType = ItemContainerType.unspecified;
  DateTime _acquiredAt = DateTime.now();
  DateTime? _expiresAt;
  ConsumableState _state = ConsumableState.unknown;
  String? _error;
  bool _isSaving = false;

  double get initialQuantity => _initialQuantity;
  double get currentQuantity => _currentQuantity;
  ItemUnit get unit => _unit;
  ItemContainerType get containerType => _containerType;
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

  void updateInitialQuantity(double value) {
    _initialQuantity = value;
    _error = null;
    notifyListeners();
  }

  void updateCurrentQuantity(double value) {
    _currentQuantity = value;
    _error = null;
    notifyListeners();
  }

  void updateUnit(ItemUnit value) {
    _unit = value;
    _error = null;
    notifyListeners();
  }

  void updateContainerType(ItemContainerType value) {
    _containerType = value;
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
    if (_initialQuantity < 0 || _currentQuantity < 0) {
      _error = 'Quantities must be zero or greater.';
      notifyListeners();
      return false;
    }
    if (_currentQuantity > _initialQuantity) {
      _error = 'Current quantity cannot be greater than initial quantity.';
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
          initialQuantity: _initialQuantity,
          currentQuantity: _currentQuantity,
          unit: _unit,
          containerType: _containerType,
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
