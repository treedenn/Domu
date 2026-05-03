import 'dart:io';

import '../http/api_error.dart';

String userErrorMessage(Object? error) {
  if (error is ApiError) {
    final int? statusCode = error.statusCode;
    if (statusCode == null) {
      return 'Could not contact the server. Check your connection and try again.';
    }
    if (statusCode == HttpStatus.unauthorized ||
        statusCode == HttpStatus.forbidden) {
      return 'Your session could not access this. Sign in again and try once more.';
    }
    if (statusCode == HttpStatus.notFound) {
      return 'We could not find what you asked for. It may have been moved or deleted.';
    }
    if (statusCode >= HttpStatus.internalServerError) {
      return 'The server ran into a problem. Try again in a moment.';
    }

    return 'The request could not be completed. Check your input and try again.';
  }

  if (error is SocketException) {
    return 'Could not contact the server. Check your connection and try again.';
  }

  if (error is FormatException) {
    return 'The server sent a response the app could not read.';
  }

  return 'Something went wrong. Try again.';
}
