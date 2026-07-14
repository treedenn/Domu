import 'package:flutter/foundation.dart';

import '../domain/household.dart';
import '../domain/household_repository.dart';

class HouseholdsViewModel extends ChangeNotifier {
  HouseholdsViewModel(this._repository);

  final HouseholdRepository _repository;
  List<Household> _households = const [];
  String? _selectedHouseholdId;
  String? _errorMessage;
  String? _message;
  bool _isLoading = false;
  bool _isRefreshing = false;
  bool _isMutating = false;

  List<Household> get households => List.unmodifiable(_households);
  Household? get selectedHousehold => _households.cast<Household?>().firstWhere(
    (household) => household?.id == _selectedHouseholdId,
    orElse: () => null,
  );
  String? get selectedHouseholdId => _selectedHouseholdId;
  String? get errorMessage => _errorMessage;
  String? get message => _message;
  bool get isLoading => _isLoading;
  bool get isRefreshing => _isRefreshing;
  bool get isMutating => _isMutating;
  bool get isEmpty =>
      !_isLoading && _households.isEmpty && _errorMessage == null;

  void clearMessage() {
    _message = null;
    notifyListeners();
  }

  Future<void> load() => _load(initial: _households.isEmpty);

  Future<void> refresh() => _load(initial: false);

  Future<void> _load({required bool initial}) async {
    if (_isLoading || _isRefreshing) return;
    if (initial) {
      _isLoading = true;
    } else {
      _isRefreshing = true;
    }
    _errorMessage = null;
    notifyListeners();
    try {
      _households = await _repository.getHouseholds();
      if (!_households.any(
        (household) => household.id == _selectedHouseholdId,
      )) {
        _selectedHouseholdId = null;
      }
    } on HouseholdRepositoryException catch (error) {
      _errorMessage = error.message;
    } catch (_) {
      _errorMessage = 'Unable to load households. Please try again.';
    } finally {
      _isLoading = false;
      _isRefreshing = false;
      notifyListeners();
    }
  }

  void selectHousehold(Household household) {
    _selectedHouseholdId = household.id;
    notifyListeners();
  }

  Future<bool> createHousehold({
    required String name,
    required String ownerDisplayName,
  }) => _mutate(
    () => _repository.createHousehold(
      name: name,
      ownerDisplayName: ownerDisplayName,
    ),
  );

  Future<bool> renameHousehold({required String id, required String name}) =>
      _mutate(() => _repository.updateHousehold(id: id, name: name));

  Future<bool> deleteHousehold(String id) async {
    if (_isMutating) return false;
    _beginMutation();
    try {
      await _repository.deleteHousehold(id);
      if (_selectedHouseholdId == id) _selectedHouseholdId = null;
      await _reloadAfterMutation();
      _message = 'Household deleted.';
      return true;
    } on HouseholdRepositoryException catch (error) {
      _message = error.message;
      return false;
    } catch (_) {
      _message = 'Unable to delete the household. Please try again.';
      return false;
    } finally {
      _isMutating = false;
      notifyListeners();
    }
  }

  Future<bool> _mutate(Future<Household> Function() action) async {
    if (_isMutating) return false;
    _beginMutation();
    try {
      await action();
      await _reloadAfterMutation();
      _message = 'Household saved.';
      return true;
    } on HouseholdRepositoryException catch (error) {
      _message = error.message;
      return false;
    } catch (_) {
      _message = 'Unable to save the household. Please try again.';
      return false;
    } finally {
      _isMutating = false;
      notifyListeners();
    }
  }

  void _beginMutation() {
    _isMutating = true;
    _message = null;
    notifyListeners();
  }

  Future<void> _reloadAfterMutation() async {
    _households = await _repository.getHouseholds();
    if (!_households.any((household) => household.id == _selectedHouseholdId)) {
      _selectedHouseholdId = null;
    }
  }
}
