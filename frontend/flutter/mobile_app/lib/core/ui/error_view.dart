import 'package:flutter/material.dart';

import '../../app/theme/tokens.dart';
import '../errors/user_error_message.dart';
import '../logging/logger.dart';

class ErrorView extends StatefulWidget {
  const ErrorView({
    required this.title,
    this.message,
    this.error,
    this.stackTrace,
    this.onRetry,
    super.key,
  });

  final String title;
  final String? message;
  final Object? error;
  final StackTrace? stackTrace;
  final VoidCallback? onRetry;

  @override
  State<ErrorView> createState() => _ErrorViewState();
}

class _ErrorViewState extends State<ErrorView> {
  static const Logger _logger = Logger();

  @override
  void initState() {
    super.initState();
    _logError();
  }

  @override
  void didUpdateWidget(ErrorView oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.error != widget.error ||
        oldWidget.stackTrace != widget.stackTrace ||
        oldWidget.title != widget.title) {
      _logError();
    }
  }

  void _logError() {
    final Object? error = widget.error;
    if (error == null) {
      return;
    }

    _logger.error(widget.title, error: error, stackTrace: widget.stackTrace);
  }

  @override
  Widget build(BuildContext context) {
    final String message = widget.message ?? userErrorMessage(widget.error);

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            Icon(
              Icons.error_outline,
              size: 48,
              color: Theme.of(context).colorScheme.error,
            ),
            const SizedBox(height: AppSpacing.md),
            Text(widget.title, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.sm),
            Text(message, textAlign: TextAlign.center),
            if (widget.onRetry != null) ...<Widget>[
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: widget.onRetry,
                child: const Text('Try again'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
