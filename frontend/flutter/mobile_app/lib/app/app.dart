import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'bootstrap/app_bootstrap.dart';
import 'bootstrap/app_config.dart';
import 'routing/router.dart';
import 'theme/app_theme.dart';
import '../features/auth/presentation/controllers/auth_controller.dart';
import '../features/households/data/households_repository.dart';
import '../features/households/data/members_repository.dart';
import '../features/items/data/items_repository.dart';
import '../features/search/data/search_repository.dart';
import '../features/spaces/data/spaces_repository.dart';

class DomuApp extends StatelessWidget {
  const DomuApp({required this.bootstrap, super.key});

  final AppBootstrap bootstrap;

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        Provider<AppConfig>.value(value: bootstrap.config),
        ChangeNotifierProvider<AuthController>.value(
          value: bootstrap.authController,
        ),
        Provider<HouseholdsRepository>.value(
          value: bootstrap.householdsRepository,
        ),
        Provider<MembersRepository>.value(value: bootstrap.membersRepository),
        Provider<SpacesRepository>.value(value: bootstrap.spacesRepository),
        Provider<ItemsRepository>.value(value: bootstrap.itemsRepository),
        Provider<SearchRepository>.value(value: bootstrap.searchRepository),
      ],
      child: Builder(
        builder: (BuildContext context) {
          final AppConfig config = context.read<AppConfig>();
          final AuthController authController = context.read<AuthController>();

          return MaterialApp.router(
            title: config.appName,
            theme: AppTheme.light(),
            darkTheme: AppTheme.dark(),
            themeMode: ThemeMode.system,
            routerConfig: buildRouter(authController),
          );
        },
      ),
    );
  }
}
