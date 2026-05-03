import 'package:flutter/foundation.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/members_repository.dart';
import '../../domain/member.dart';

class MembersViewModel extends ChangeNotifier {
  MembersViewModel({
    required String householdId,
    required MembersRepository repository,
    AuthSession? session,
  }) : _householdId = householdId,
       _repository = repository,
       _session = session {
    load();
  }

  String _householdId;
  MembersRepository _repository;
  AuthSession? _session;
  bool _isLoading = false;
  Object? _error;
  StackTrace? _stackTrace;
  List<Member> _members = const <Member>[];

  bool get isLoading => _isLoading;
  Object? get error => _error;
  StackTrace? get stackTrace => _stackTrace;
  List<Member> get members => _members;
  AuthSession? get session => _session;

  void updateDependencies({
    required String householdId,
    required MembersRepository repository,
    AuthSession? session,
  }) {
    final bool shouldReload =
        _householdId != householdId ||
        _repository != repository ||
        _session != session;
    _householdId = householdId;
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
      _members = const <Member>[];
      _isLoading = false;
      notifyListeners();
      return;
    }

    try {
      _members = await _repository.getMembers(
        session: session,
        householdId: _householdId,
      );
    } catch (error, stackTrace) {
      _error = error;
      _stackTrace = stackTrace;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> invite({required String email, required MemberRole role}) async {
    final AuthSession? session = _session;
    if (session == null) {
      return;
    }
    await _repository.invite(
      session: session,
      householdId: _householdId,
      email: email,
      role: role,
    );
    await load();
  }
}
