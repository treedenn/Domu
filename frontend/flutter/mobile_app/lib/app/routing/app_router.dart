import 'package:flutter/material.dart';

import '../../features/households/presentation/screens/household_list_screen.dart';
import '../../features/spaces/presentation/screens/space_list_screen.dart';
import '../../features/users/presentation/screens/user_profile_screen.dart';

class AppRouter {
  static const String householdsRoute = '/households';
  static const String spacesRoute = '/spaces';
  static const String usersRoute = '/users';

  static Route<dynamic> onGenerateRoute(RouteSettings settings) {
    final Widget screen = switch (settings.name) {
      householdsRoute => const HouseholdListScreen(),
      spacesRoute => const SpaceListScreen(),
      usersRoute => const UserProfileScreen(),
      _ => const HouseholdListScreen(),
    };

    return MaterialPageRoute<void>(
      builder: (BuildContext context) => screen,
      settings: settings,
    );
  }
}
