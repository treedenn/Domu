import '../domain/user_profile.dart';

abstract class UsersRepository {
  Future<UserProfile?> getCurrentUser();
}
