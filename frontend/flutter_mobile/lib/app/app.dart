import 'package:domu_mobile/app/router/app_router.dart';
import 'package:domu_mobile/app/theme/app_theme.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:domu_mobile/features/members/ui/members_view_model.dart';
import 'package:flutter/material.dart';

class DomuApp extends StatefulWidget {
  const DomuApp({
    super.key,
    required this.authViewModel,
    required this.householdsViewModel,
    required this.membersViewModel,
  });

  final AuthViewModel authViewModel;
  final HouseholdsViewModel householdsViewModel;
  final MembersViewModel membersViewModel;

  @override
  State<DomuApp> createState() => _DomuAppState();
}

class _DomuAppState extends State<DomuApp> {
  late final AppRouter _appRouter = AppRouter(
    widget.authViewModel,
    householdsViewModel: widget.householdsViewModel,
    membersViewModel: widget.membersViewModel,
  );

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'Domu',
      theme: AppTheme.light,
      routerConfig: _appRouter.router,
    );
  }
}
