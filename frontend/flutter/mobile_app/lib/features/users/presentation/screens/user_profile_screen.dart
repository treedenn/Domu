import 'package:flutter/material.dart';

import '../../../../core/ui/empty_view.dart';

class UserProfileScreen extends StatelessWidget {
  const UserProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Profile')),
      body: const EmptyView(
        title: 'No user profile yet',
        message: 'Map the backend user bootstrap data into a profile screen here.',
      ),
    );
  }
}
