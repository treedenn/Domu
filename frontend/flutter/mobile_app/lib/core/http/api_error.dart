class ApiError implements Exception {
  const ApiError(this.message, {this.statusCode, this.uri, this.cause});

  final String message;
  final int? statusCode;
  final Uri? uri;
  final Object? cause;

  @override
  String toString() {
    final StringBuffer buffer = StringBuffer(
      'ApiError(statusCode: $statusCode, message: $message',
    );
    if (uri != null) {
      buffer.write(', uri: $uri');
    }
    if (cause != null) {
      buffer.write(', cause: $cause');
    }
    buffer.write(')');
    return buffer.toString();
  }
}
