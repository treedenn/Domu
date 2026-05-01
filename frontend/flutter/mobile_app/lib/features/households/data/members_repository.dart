import '../../../core/auth/auth_session.dart';
import '../../../core/http/api_client.dart';
import '../domain/member.dart';

abstract class MembersRepository {
  Future<List<Member>> getMembers({
    required AuthSession session,
    required String householdId,
  });

  Future<void> invite({
    required AuthSession session,
    required String householdId,
    required String email,
    required MemberRole role,
  });
}

class ApiMembersRepository implements MembersRepository {
  const ApiMembersRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<Member>> getMembers({
    required AuthSession session,
    required String householdId,
  }) async {
    final Object? json = await _apiClient.getJson(
      '/api/v1/households/$householdId/members',
      session: session,
    );

    if (json is! List<Object?>) {
      throw const FormatException('Expected a household member list.');
    }

    return json
        .whereType<Map<String, Object?>>()
        .map(Member.fromJson)
        .toList(growable: false);
  }

  @override
  Future<void> invite({
    required AuthSession session,
    required String householdId,
    required String email,
    required MemberRole role,
  }) async {
    await _apiClient.postJson(
      '/api/v1/households/$householdId/invitations',
      session: session,
      body: <String, Object?>{
        'email': email,
        'role': role.toJson(),
      },
    );
  }
}
