import 'package:flutter/material.dart';

import '../../app/theme/tokens.dart';

class LoadingView extends StatelessWidget {
  const LoadingView({this.label, super.key});

  final String? label;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          const CircularProgressIndicator(),
          if (label != null) ...<Widget>[
            const SizedBox(height: AppSpacing.md),
            Text(label!),
          ],
        ],
      ),
    );
  }
}
