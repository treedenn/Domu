import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/households_repository.dart';
import '../../domain/household.dart';

class HouseholdListViewModel extends ChangeNotifier {
  HouseholdListViewModel({
    HouseholdsRepository? repository,
    AuthSession? session,
  }) : _repository = repository,
       _session = session {
    load();
  }

  HouseholdsRepository? _repository;
  AuthSession? _session;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  List<Household> _households = const <Household>[];

  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  List<Household> get households => _households;

  void updateDependencies({
    HouseholdsRepository? repository,
    AuthSession? session,
  }) {
    final bool shouldReload = _repository != repository || _session != session;
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

    final HouseholdsRepository? repository = _repository;
    final AuthSession? session = _session;
    if (repository == null || session == null) {
      _households = const <Household>[];
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      _households = await repository.getHouseholds(session);
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
