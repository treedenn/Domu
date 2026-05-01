import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../app/theme/tokens.dart';

class Breadcrumbs extends StatelessWidget {
  const Breadcrumbs({required this.householdName, super.key});

  final String householdName;

  @override
  Widget build(BuildContext context) {
    final GoRouterState state = GoRouterState.of(context);
    final List<String> segments = state.uri.pathSegments;
    if (segments.length <= 3) {
      return const SizedBox(height: 36);
    }

    final List<_Crumb> crumbs = <_Crumb>[
      _Crumb(label: householdName, path: '/${segments.take(3).join('/')}'),
    ];
    if (segments.length >= 5) {
      crumbs.add(_Crumb(label: 'Space', path: '/${segments.take(5).join('/')}'));
    }
    if (segments.length >= 7) {
      crumbs.add(_Crumb(label: 'Item', path: '/${segments.take(7).join('/')}'));
    }

    return SizedBox(
      height: 36,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        itemCount: crumbs.length,
        separatorBuilder: (BuildContext context, int index) => const Padding(
          padding: EdgeInsets.symmetric(horizontal: AppSpacing.xs),
          child: Icon(Icons.chevron_right, size: 16),
        ),
        itemBuilder: (BuildContext context, int index) {
          final _Crumb crumb = crumbs[index];
          final bool current = index == crumbs.length - 1;
          return TextButton(
            onPressed: current
                ? null
                : () => context.go(
                      '${crumb.path}?name=${Uri.encodeQueryComponent(householdName)}',
                    ),
            child: Text(crumb.label),
          );
        },
      ),
    );
  }
}

class _Crumb {
  const _Crumb({required this.label, required this.path});

  final String label;
  final String path;
}
