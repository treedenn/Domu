import 'package:flutter/material.dart';

import 'bootstrap/app_bootstrap.dart';
import 'routing/app_router.dart';
import 'theme/app_theme.dart';
import '../core/ui/loading_view.dart';
import '../features/auth/presentation/screens/login_screen.dart';
import '../features/households/presentation/screens/household_list_screen.dart';

class DomuApp extends StatelessWidget {
  const DomuApp({required this.bootstrap, super.key});

  final AppBootstrap bootstrap;

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: bootstrap.authController,
      builder: (BuildContext context, Widget? child) {
        final authState = bootstrap.authController.state;

        return MaterialApp(
          title: bootstrap.config.appName,
          theme: AppTheme.light(),
          onGenerateRoute: AppRouter.onGenerateRoute,
          home: switch ((authState.isInitializing, authState.isAuthenticated)) {
            (true, _) => const Scaffold(
                body: LoadingView(label: 'Restoring session...'),
              ),
            (_, true) => HouseholdListScreen(
                onOpenSpaces: (BuildContext context) {
                  Navigator.of(context).pushNamed(AppRouter.spacesRoute);
                },
                onOpenUsers: (BuildContext context) {
                  Navigator.of(context).pushNamed(AppRouter.usersRoute);
                },
                onSignOut: bootstrap.authController.signOut,
              ),
            _ => LoginScreen(
                controller: bootstrap.authController,
                config: bootstrap.config,
              ),
          },
        );
      },
    );
  }
}
