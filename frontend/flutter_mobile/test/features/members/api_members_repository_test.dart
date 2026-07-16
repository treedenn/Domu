import 'dart:convert';

import 'package:domu_mobile/core/api/api_client.dart';
import 'package:domu_mobile/features/members/data/api_members_repository.dart';
import 'package:domu_mobile/features/members/domain/household_member.dart';
import 'package:domu_mobile/features/members/domain/members_repository.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  const member = <String, Object?>{
    'id': 'member-1',
    'displayName': 'Ada',
    'role': 'owner',
    'archived': false,
  };

  test('parses the member envelope and only exposes active members', () async {
    final repository = ApiMembersRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((request) async {
          expect(request.url.path, '/api/v1/households/home/members');
          return http.Response(
            jsonEncode({
              'data': [
                member,
                {...member, 'id': 'member-2', 'archived': true},
              ],
              'canManageMembers': true,
            }),
            200,
          );
        }),
      ),
    );

    final result = await repository.getMembers('home');

    expect(result.canManageMembers, isTrue);
    expect(result.members, hasLength(1));
    expect(result.members.single.role, HouseholdMemberRole.owner);
  });

  test(
    'serializes invitations and member archives with API payloads',
    () async {
      final requests = <http.Request>[];
      final repository = ApiMembersRepository(
        ApiClient(
          baseUrl: 'https://api.example.test',
          accessToken: () async => 'token',
          httpClient: MockClient((request) async {
            requests.add(request);
            if (request.method == 'POST') {
              return http.Response(
                jsonEncode({
                  'data': {
                    'id': 'invitation-1',
                    'displayName': 'Grace',
                    'email': 'grace@example.test',
                    'role': 'admin',
                  },
                }),
                201,
              );
            }
            return http.Response('', 200);
          }),
        ),
      );

      await repository.createInvitation(
        householdId: 'home',
        displayName: 'Grace',
        email: 'grace@example.test',
        role: HouseholdMemberRole.admin,
      );
      await repository.archiveMember(
        householdId: 'home',
        member: const HouseholdMember(
          id: 'member-2',
          displayName: 'Grace',
          role: HouseholdMemberRole.admin,
          archived: false,
        ),
      );

      expect(requests[0].url.path, '/api/v1/households/home/invitations');
      expect(jsonDecode(requests[0].body), {
        'displayName': 'Grace',
        'email': 'grace@example.test',
        'role': 'admin',
      });
      expect(requests[1].url.path, '/api/v1/households/home/members/member-2');
      expect(jsonDecode(requests[1].body), {
        'displayName': 'Grace',
        'role': 'admin',
        'archived': true,
      });
    },
  );

  test('parses pending invitations from the data envelope', () async {
    final repository = ApiMembersRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((request) async {
          expect(request.url.path, '/api/v1/households/home/invitations');
          return http.Response(
            jsonEncode({
              'data': [
                {
                  'id': 'invitation-1',
                  'displayName': 'Grace',
                  'email': 'grace@example.test',
                  'role': 'admin',
                },
              ],
            }),
            200,
          );
        }),
      ),
    );

    final invitations = await repository.getPendingInvitations('home');

    expect(invitations, hasLength(1));
    expect(invitations.single.displayName, 'Grace');
    expect(invitations.single.role, HouseholdMemberRole.admin);
  });

  test('maps API failures to a members repository exception', () async {
    final repository = ApiMembersRepository(
      ApiClient(
        baseUrl: 'https://api.example.test',
        accessToken: () async => 'token',
        httpClient: MockClient((_) async => http.Response('', 403)),
      ),
    );

    expect(
      repository.getMembers('home'),
      throwsA(isA<MembersRepositoryException>()),
    );
  });
}
