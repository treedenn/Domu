import 'dart:async';

import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../../items/domain/item.dart';
import '../../../spaces/domain/space.dart';
import '../../data/search_repository.dart';
import '../../domain/search_engine.dart';
import '../../domain/search_query.dart';

class SearchViewModel extends ChangeNotifier {
  SearchViewModel({
    required String householdId,
    required SearchRepository repository,
    AuthSession? session,
    String? initialExpiringFilter,
  }) : _householdId = householdId,
       _repository = repository,
       _session = session,
       _initialExpiringFilter = initialExpiringFilter {
    if (_expiringDays != null) {
      _runSearch();
    }
  }

  String _householdId;
  SearchRepository _repository;
  AuthSession? _session;
  String? _initialExpiringFilter;
  Timer? _debounce;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  SearchResults? _results;
  String _query = '';

  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  SearchResults? get results => _results;
  String get query => _query;
  bool get emptyQuery => _query.trim().isEmpty && _expiringDays == null;

  int? get _expiringDays => _initialExpiringFilter == '7d' ? 7 : null;

  void updateDependencies({
    required String householdId,
    required SearchRepository repository,
    AuthSession? session,
    String? initialExpiringFilter,
  }) {
    final bool shouldSearch =
        _householdId != householdId ||
        _repository != repository ||
        _session != session ||
        _initialExpiringFilter != initialExpiringFilter;

    _householdId = householdId;
    _repository = repository;
    _session = session;
    _initialExpiringFilter = initialExpiringFilter;

    if (shouldSearch && !emptyQuery) {
      _runSearch();
    } else {
      notifyListeners();
    }
  }

  void updateQuery(String value) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 200), () {
      _query = value;
      if (emptyQuery) {
        _results = null;
        _error = null;
        _stackTrace = null;
        _isLoading = false;
        notifyListeners();
        return;
      }
      _runSearch();
    });
  }

  Future<void> retry() => _runSearch();

  Future<void> _runSearch() async {
    _isLoading = true;
    _error = null;
    _stackTrace = null;
    notifyListeners();

    final AuthSession? session = _session;
    if (session == null) {
      _results = const SearchResults(spaces: <Space>[], items: <Item>[]);
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      _results = await _repository.search(
        session: session,
        householdId: _householdId,
        query: SearchQuery(text: _query, expiringWithinDays: _expiringDays),
      );
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  @override
  void dispose() {
    _debounce?.cancel();
    super.dispose();
  }
}
