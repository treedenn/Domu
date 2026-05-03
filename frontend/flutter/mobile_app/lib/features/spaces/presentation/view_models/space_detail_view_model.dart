import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../../../features/items/data/items_repository.dart';
import '../../data/spaces_repository.dart';
import '../../domain/space.dart';

class SpaceDetailViewModel extends ChangeNotifier {
  SpaceDetailViewModel({
    required String householdId,
    required String householdName,
    required String spaceId,
    required SpacesRepository spacesRepository,
    required ItemsRepository itemsRepository,
    AuthSession? session,
  }) : _householdId = householdId,
       _householdName = householdName,
       _spaceId = spaceId,
       _spacesRepository = spacesRepository,
       _itemsRepository = itemsRepository,
       _session = session {
    loadChildren();
  }

  String _householdId;
  String _householdName;
  String _spaceId;
  SpacesRepository _spacesRepository;
  ItemsRepository _itemsRepository;
  AuthSession? _session;
  bool _isLoadingChildren = false;
  Object? _childrenError;
  StackTrace? _childrenStackTrace;
  List<Space> _children = const <Space>[];

  String get householdId => _householdId;
  String get householdName => _householdName;
  String get spaceId => _spaceId;
  ItemsRepository get itemsRepository => _itemsRepository;
  AuthSession? get session => _session;
  bool get isLoadingChildren => _isLoadingChildren;
  Object? get childrenError => _childrenError;
  StackTrace? get childrenStackTrace => _childrenStackTrace;
  List<Space> get children => _children;

  void updateDependencies({
    required String householdId,
    required String householdName,
    required String spaceId,
    required SpacesRepository spacesRepository,
    required ItemsRepository itemsRepository,
    AuthSession? session,
  }) {
    final bool shouldReload =
        _householdId != householdId ||
        _spaceId != spaceId ||
        _spacesRepository != spacesRepository ||
        _session != session;

    _householdId = householdId;
    _householdName = householdName;
    _spaceId = spaceId;
    _spacesRepository = spacesRepository;
    _itemsRepository = itemsRepository;
    _session = session;

    if (shouldReload) {
      loadChildren();
    } else {
      notifyListeners();
    }
  }

  Future<void> loadChildren() async {
    _isLoadingChildren = true;
    _childrenError = null;
    _childrenStackTrace = null;
    notifyListeners();

    final AuthSession? session = _session;
    if (session == null) {
      _children = const <Space>[];
      _isLoadingChildren = false;
      notifyListeners();
      return;
    }

    try {
      final SpacePage page = await _spacesRepository.getSpaces(
        session: session,
        householdId: _householdId,
        parentId: _spaceId,
      );
      _children = page.spaces;
    } catch (error, stackTrace) {
      _childrenError = error;
      _childrenStackTrace = stackTrace;
    } finally {
      _isLoadingChildren = false;
      notifyListeners();
    }
  }
}
