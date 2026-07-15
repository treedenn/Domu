import 'dart:convert';

import '../../../core/api/api_client.dart';
import '../domain/household_member.dart';
import '../domain/members_repository.dart';
import '../domain/members_result.dart';
import '../domain/pending_invitation.dart';
import 'members_dto.dart';

class ApiMembersRepository implements MembersRepository {
  ApiMembersRepository(this._client);

  final ApiClient _client;
  static const _path = '/api/v1/households';

  @override
  Future<MembersResult> getMembers(String householdId) async {
    final response = await _request(
      () => _client.get('$_path/$householdId/members'),
    );
    try {
      final json = _decode(response.body);
      if (json is List) {
        return MembersResult(
          members: _membersFromJson(json),
          canManageMembers: false,
        );
      }
      final envelope = _asMap(json);
      final rawMembers = envelope['members'];
      final canManageMembers = envelope['canManageMembers'];
      if (rawMembers is! List || canManageMembers is! bool) {
        throw const FormatException('Invalid member list.');
      }
      return MembersResult(
        members: _membersFromJson(rawMembers),
        canManageMembers: canManageMembers,
      );
    } on FormatException {
      throw const MembersRepositoryException(
        'Domu returned an invalid member list.',
      );
    }
  }

  @override
  Future<List<PendingInvitation>> getPendingInvitations(
    String householdId,
  ) async {
    final response = await _request(
      () => _client.get('$_path/$householdId/invitations'),
    );
    try {
      final json = _decode(response.body);
      if (json is! List) {
        throw const FormatException('Invalid invitation list.');
      }
      return json
          .map((item) => PendingInvitationDto.fromJson(_asMap(item)).toDomain())
          .toList(growable: false);
    } on FormatException {
      throw const MembersRepositoryException(
        'Domu returned an invalid invitation list.',
      );
    }
  }

  @override
  Future<PendingInvitation> createInvitation({
    required String householdId,
    required String displayName,
    required String email,
    required HouseholdMemberRole role,
  }) async {
    final response = await _request(
      () => _client.post(
        '$_path/$householdId/invitations',
        body: {'displayName': displayName, 'email': email, 'role': role.name},
      ),
    );
    try {
      return PendingInvitationDto.fromJson(
        _asMap(_decode(response.body)),
      ).toDomain();
    } on FormatException {
      throw const MembersRepositoryException(
        'Domu returned an invalid invitation response.',
      );
    }
  }

  @override
  Future<void> archiveMember({
    required String householdId,
    required HouseholdMember member,
  }) async {
    await _request(
      () => _client.put(
        '$_path/$householdId/members/${member.id}',
        body: {
          'displayName': member.displayName,
          'role': member.role.name,
          'archived': true,
        },
      ),
    );
  }

  Future<ApiResponse> _request(Future<ApiResponse> Function() action) async {
    try {
      final response = await action();
      if (!response.isSuccess) {
        throw MembersRepositoryException(_messageFor(response));
      }
      return response;
    } on MembersRepositoryException {
      rethrow;
    } on ApiClientException catch (error) {
      throw MembersRepositoryException(error.message);
    } catch (_) {
      throw const MembersRepositoryException(
        'Unable to complete that request. Please try again.',
      );
    }
  }

  static dynamic _decode(String body) => jsonDecode(body);

  static List<HouseholdMember> _membersFromJson(List json) => json
      .map((item) => HouseholdMemberDto.fromJson(_asMap(item)).toDomain())
      .where((member) => !member.archived)
      .toList(growable: false);

  static Map<String, dynamic> _asMap(Object? value) {
    if (value is Map<String, dynamic>) return value;
    if (value is Map) return Map<String, dynamic>.from(value);
    throw const FormatException('Expected an object.');
  }

  static String _messageFor(ApiResponse response) =>
      switch (response.statusCode) {
        400 => 'Please check the member details and try again.',
        401 => 'Your session has expired. Please sign in again.',
        403 => 'You do not have permission to manage members.',
        404 => 'That household or member could not be found.',
        _ => 'Unable to complete that request. Please try again.',
      };
}
