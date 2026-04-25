import 'package:flutter/material.dart';

import '../../../../core/ui/empty_view.dart';

class SpaceListScreen extends StatelessWidget {
  const SpaceListScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Spaces')),
      body: const EmptyView(
        title: 'No spaces yet',
        message: 'Add the mobile projection for spaces and attach the repository here.',
      ),
    );
  }
}
