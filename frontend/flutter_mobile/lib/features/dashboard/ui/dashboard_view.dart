import 'package:flutter/material.dart';

class DashboardView extends StatelessWidget {
  const DashboardView({super.key, this.onSignOut});

  final Future<void> Function()? onSignOut;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Domu'),
        actions: [
          IconButton(
            tooltip: 'Sign out',
            onPressed: onSignOut,
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: const Center(child: Text('Your home, organised.')),
    );
  }
}
