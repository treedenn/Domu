import 'dart:async';

import 'package:flutter/widgets.dart';

import 'app/app.dart';
import 'app/bootstrap/app_bootstrap.dart';
import 'core/logging/logger.dart';

void main() {
  const Logger logger = Logger();

  runZonedGuarded(
    () async {
      WidgetsFlutterBinding.ensureInitialized();
      FlutterError.onError = (FlutterErrorDetails details) {
        FlutterError.presentError(details);
        logger.error(
          'Unhandled Flutter error',
          error: details.exception,
          stackTrace: details.stack,
        );
      };

      final AppBootstrap bootstrap = await AppBootstrap.initialize();

      runApp(DomuApp(bootstrap: bootstrap));
    },
    (Object error, StackTrace stackTrace) {
      logger.error(
        'Unhandled Dart error',
        error: error,
        stackTrace: stackTrace,
      );
    },
  );
}
