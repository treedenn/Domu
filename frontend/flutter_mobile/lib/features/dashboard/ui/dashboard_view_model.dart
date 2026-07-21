import 'package:flutter/foundation.dart';

import '../../households/domain/household_expiration.dart';
import '../../households/domain/household_repository.dart';

class DashboardViewModel extends ChangeNotifier {
  DashboardViewModel(this._repository, {DateTime Function()? now})
    : _now = now ?? DateTime.now;

  final HouseholdRepository _repository;
  final DateTime Function() _now;
  HouseholdExpirations? _expirations;
  String? _errorMessage;
  bool _isLoading = false;

  HouseholdExpirations? get expirations => _expirations;
  String? get errorMessage => _errorMessage;
  bool get isLoading => _isLoading;

  Future<void> load(String householdId) async {
    if (_isLoading) return;
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _expirations = await _repository.getHouseholdExpirations(
        householdId: householdId,
        upcomingUntil: _now().add(const Duration(days: 30)),
      );
    } on HouseholdRepositoryException catch (error) {
      _errorMessage = error.message;
    } catch (_) {
      _errorMessage = 'Unable to load expirations. Please try again.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
