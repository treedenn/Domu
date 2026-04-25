import '../domain/user_profile.dart';

abstract class UsersRepository {
  Future<UserProfile?> getCurrentUser();
}

class StubUsersRepository implements UsersRepository {
  @override
  Future<UserProfile?> getCurrentUser() async {
    return null;
  }
}
