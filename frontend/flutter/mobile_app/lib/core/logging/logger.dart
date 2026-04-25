import 'package:flutter/foundation.dart';

class Logger {
  const Logger();

  void info(String message) {
    debugPrint('[INFO] $message');
  }

  void error(String message) {
    debugPrint('[ERROR] $message');
  }
}
