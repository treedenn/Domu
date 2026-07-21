import 'package:flutter/material.dart';

import '../../households/domain/household_expiration.dart';
import 'dashboard_view_model.dart';

class DashboardView extends StatefulWidget {
  const DashboardView({
    super.key,
    required this.householdId,
    required this.viewModel,
  });

  final String householdId;
  final DashboardViewModel viewModel;

  @override
  State<DashboardView> createState() => _DashboardViewState();
}

class _DashboardViewState extends State<DashboardView> {
  @override
  void initState() {
    super.initState();
    widget.viewModel.load(widget.householdId);
  }

  @override
  void didUpdateWidget(covariant DashboardView oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.householdId != widget.householdId) {
      widget.viewModel.load(widget.householdId);
    }
  }

  @override
  Widget build(BuildContext context) =>
      AnimatedBuilder(animation: widget.viewModel, builder: (_, _) => _body());

  Widget _body() {
    final vm = widget.viewModel;
    if (vm.isLoading && vm.expirations == null) {
      return const Center(child: CircularProgressIndicator());
    }
    if (vm.errorMessage case final error?) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(error, textAlign: TextAlign.center),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => vm.load(widget.householdId),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }
    final expirations = vm.expirations;
    if (expirations == null) return const SizedBox.expand();
    return RefreshIndicator(
      onRefresh: () => vm.load(widget.householdId),
      child: ListView(
        padding: const EdgeInsets.symmetric(vertical: 16),
        children: [
          _section('Expired', expirations.expired, expired: true),
          const SizedBox(height: 16),
          _section('Expiring in the next 30 days', expirations.upcoming),
        ],
      ),
    );
  }

  Widget _section(
    String title,
    List<HouseholdExpiration> expirations, {
    bool expired = false,
  }) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16),
        child: Text(title, style: Theme.of(context).textTheme.titleLarge),
      ),
      const SizedBox(height: 8),
      if (expirations.isEmpty)
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 16),
          child: Text('Nothing here.'),
        )
      else
        for (final expiration in expirations)
          ListTile(
            leading: Icon(
              expired ? Icons.warning_amber_rounded : Icons.event_outlined,
              color: expired ? Theme.of(context).colorScheme.error : null,
            ),
            title: Text(expiration.itemName),
            subtitle: Text(
              '${expiration.spaceName} · ${_date(expiration.expirationDate)}',
            ),
            trailing: Text('${expiration.count}'),
          ),
    ],
  );

  String _date(DateTime value) =>
      MaterialLocalizations.of(context).formatMediumDate(value.toLocal());
}
