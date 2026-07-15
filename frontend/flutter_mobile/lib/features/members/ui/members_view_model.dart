import 'package:flutter/foundation.dart';

import '../domain/household_member.dart';
import '../domain/members_repository.dart';
import '../domain/pending_invitation.dart';

class MembersViewModel extends ChangeNotifier {
  MembersViewModel(this._repository);

  final MembersRepository _repository;
  List<HouseholdMember> _members = const [];
  List<PendingInvitation> _pendingInvitations = const [];
  String? _householdId;
  String? _errorMessage;
  String? _message;
  bool _canManageMembers = false;
  bool _isLoading = false;
  bool _isRefreshing = false;
  bool _isMutating = false;

  List<HouseholdMember> get members => List.unmodifiable(_members);
  List<PendingInvitation> get pendingInvitations =>
      List.unmodifiable(_pendingInvitations);
  String? get errorMessage => _errorMessage;
  String? get message => _message;
  bool get canManageMembers => _canManageMembers;
  bool get isLoading => _isLoading;
  bool get isRefreshing => _isRefreshing;
  bool get isMutating => _isMutating;
  bool get isEmpty => !_isLoading && _members.isEmpty && _errorMessage == null;

  void clearMessage() {
    _message = null;
    notifyListeners();
  }

  Future<void> load(String householdId) =>
      _load(householdId, initial: _householdId != householdId);

  Future<void> refresh() {
    final householdId = _householdId;
    return householdId == null
        ? Future<void>.value()
        : _load(householdId, initial: false);
  }

  Future<void> _load(String householdId, {required bool initial}) async {
    if (_isLoading || _isRefreshing) {
      return;
    }
    _householdId = householdId;
    if (initial) {
      _isLoading = true;
    } else {
      _isRefreshing = true;
    }
    _errorMessage = null;
    notifyListeners();
    try {
      final result = await _repository.getMembers(householdId);
      _members = _sortMembers(result.members);
      _canManageMembers = result.canManageMembers;
      _pendingInvitations = result.canManageMembers
          ? await _repository.getPendingInvitations(householdId)
          : const [];
    } on MembersRepositoryException catch (error) {
      _errorMessage = error.message;
    } catch (_) {
      _errorMessage = 'Unable to load members. Please try again.';
    } finally {
      _isLoading = false;
      _isRefreshing = false;
      notifyListeners();
    }
  }

  Future<bool> invite({
    required String displayName,
    required String email,
    required HouseholdMemberRole role,
  }) => _mutate(
    successMessage: 'Invitation sent.',
    action: (householdId) => _repository.createInvitation(
      householdId: householdId,
      displayName: displayName,
      email: email,
      role: role,
    ),
  );

  Future<bool> archive(HouseholdMember member) => _mutate(
    successMessage: '${member.displayName} removed.',
    action: (householdId) =>
        _repository.archiveMember(householdId: householdId, member: member),
  );

  Future<bool> _mutate({
    required String successMessage,
    required Future<Object?> Function(String householdId) action,
  }) async {
    final householdId = _householdId;
    if (_isMutating || householdId == null) return false;
    _isMutating = true;
    _message = null;
    notifyListeners();
    try {
      await action(householdId);
      await _reload(householdId);
      _message = successMessage;
      return true;
    } on MembersRepositoryException catch (error) {
      _message = error.message;
      return false;
    } catch (_) {
      _message = 'Unable to complete that request. Please try again.';
      return false;
    } finally {
      _isMutating = false;
      notifyListeners();
    }
  }

  Future<void> _reload(String householdId) async {
    final result = await _repository.getMembers(householdId);
    _members = _sortMembers(result.members);
    _canManageMembers = result.canManageMembers;
    _pendingInvitations = result.canManageMembers
        ? await _repository.getPendingInvitations(householdId)
        : const [];
  }

  static List<HouseholdMember> _sortMembers(List<HouseholdMember> members) {
    final sorted = members.where((member) => !member.archived).toList();
    sorted.sort((a, b) {
      final role = a.role.index.compareTo(b.role.index);
      return role != 0
          ? role
          : a.displayName.toLowerCase().compareTo(b.displayName.toLowerCase());
    });
    return List.unmodifiable(sorted);
  }
}
