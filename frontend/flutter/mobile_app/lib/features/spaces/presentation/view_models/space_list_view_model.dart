import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../../../features/items/data/items_repository.dart';
import '../../data/spaces_repository.dart';
import '../../domain/space.dart';

class SpaceListViewModel extends ChangeNotifier {
  SpaceListViewModel({
    required String householdId,
    required String householdName,
    required ItemsRepository itemsRepository,
    SpacesRepository? repository,
    AuthSession? session,
  }) : _householdId = householdId,
       _householdName = householdName,
       _itemsRepository = itemsRepository,
       _repository = repository,
       _session = session {
    load();
  }

  String _householdId;
  String _householdName;
  ItemsRepository _itemsRepository;
  SpacesRepository? _repository;
  AuthSession? _session;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  List<Space> _spaces = const <Space>[];
  String _query = '';

  String get householdId => _householdId;
  String get householdName => _householdName;
  ItemsRepository get itemsRepository => _itemsRepository;
  AuthSession? get session => _session;
  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  List<Space> get allSpaces => _spaces;
  String get query => _query;

  List<Space> get spaces {
    final String normalizedQuery = _query.toLowerCase();
    return _spaces
        .where(
          (Space space) => space.name.toLowerCase().contains(normalizedQuery),
        )
        .toList(growable: false);
  }

  void updateDependencies({
    required String householdId,
    required String householdName,
    required ItemsRepository itemsRepository,
    SpacesRepository? repository,
    AuthSession? session,
  }) {
    final bool shouldReload =
        _householdId != householdId ||
        _repository != repository ||
        _session != session;

    _householdId = householdId;
    _householdName = householdName;
    _itemsRepository = itemsRepository;
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

  Future<void> load() async {
    _isLoading = true;
    _error = null;
    _stackTrace = null;
    notifyListeners();

    final SpacesRepository? repository = _repository;
    final AuthSession? session = _session;
    if (repository == null || session == null) {
      _spaces = const <Space>[];
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      final SpacePage page = await repository.getSpaces(
        session: session,
        householdId: _householdId,
      );
      _spaces = page.spaces;
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> createSpace({required String name, String? description}) async {
    final SpacesRepository? repository = _repository;
    final AuthSession? session = _session;
    if (repository == null || session == null) {
      return;
    }

    await repository.create(
      session: session,
      householdId: _householdId,
      name: name,
      description: description,
    );
    await load();
  }
}
