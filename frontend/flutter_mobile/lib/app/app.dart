import 'package:domu_mobile/app/router/app_router.dart';
import 'package:domu_mobile/app/theme/app_theme.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/households/ui/households_view_model.dart';
import 'package:domu_mobile/features/dashboard/ui/dashboard_view_model.dart';
import 'package:domu_mobile/features/members/ui/members_view_model.dart';
import 'package:domu_mobile/features/shopping_lists/ui/shopping_list_detail_view_model.dart';
import 'package:domu_mobile/features/shopping_lists/ui/shopping_lists_view_model.dart';
import 'package:domu_mobile/features/spaces/ui/spaces_view_model.dart';
import 'package:flutter/material.dart';

class DomuApp extends StatefulWidget {
  const DomuApp({
    super.key,
    required this.authViewModel,
    required this.householdsViewModel,
    required this.membersViewModel,
    required this.shoppingListsViewModel,
    required this.shoppingListDetailViewModel,
    required this.spacesViewModel,
    required this.dashboardViewModel,
  });

  final AuthViewModel authViewModel;
  final HouseholdsViewModel householdsViewModel;
  final MembersViewModel membersViewModel;
  final ShoppingListsViewModel shoppingListsViewModel;
  final ShoppingListDetailViewModel shoppingListDetailViewModel;
  final SpacesViewModel spacesViewModel;
  final DashboardViewModel dashboardViewModel;

  @override
  State<DomuApp> createState() => _DomuAppState();
}

class _DomuAppState extends State<DomuApp> {
  late final AppRouter _appRouter = AppRouter(
    widget.authViewModel,
    householdsViewModel: widget.householdsViewModel,
    membersViewModel: widget.membersViewModel,
    shoppingListsViewModel: widget.shoppingListsViewModel,
    shoppingListDetailViewModel: widget.shoppingListDetailViewModel,
    spacesViewModel: widget.spacesViewModel,
    dashboardViewModel: widget.dashboardViewModel,
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
