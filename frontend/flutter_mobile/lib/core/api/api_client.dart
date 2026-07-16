import 'dart:convert';

import 'package:http/http.dart' as http;

/// Minimal authenticated JSON transport shared by API-backed features.
class ApiClient {
  ApiClient({
    required String baseUrl,
    required this.accessToken,
    http.Client? httpClient,
  }) : _baseUri = Uri.parse(baseUrl),
       _httpClient = httpClient ?? http.Client();

  final Uri _baseUri;
  final Future<String?> Function() accessToken;
  final http.Client _httpClient;

  Future<ApiResponse> get(String path) => _send('GET', path);

  Future<ApiResponse> post(String path, {Object? body}) =>
      _send('POST', path, body: body);

  Future<ApiResponse> put(String path, {Object? body}) =>
      _send('PUT', path, body: body);

  Future<ApiResponse> patch(String path, {Object? body}) =>
      _send('PATCH', path, body: body);

  Future<ApiResponse> delete(String path) => _send('DELETE', path);

  Future<ApiResponse> _send(String method, String path, {Object? body}) async {
    final token = await accessToken();
    if (token == null || token.isEmpty) {
      throw const ApiClientException(
        'Your session has expired. Please sign in again.',
      );
    }

    final request = http.Request(method, _baseUri.resolve(path));
    request.headers.addAll(<String, String>{
      'Accept': 'application/json',
      'Authorization': 'Bearer $token',
      if (body != null) 'Content-Type': 'application/json',
    });
    if (body != null) request.body = jsonEncode(body);

    try {
      final streamed = await _httpClient.send(request);
      final response = await http.Response.fromStream(streamed);
      return ApiResponse(response.statusCode, response.body);
    } on ApiClientException {
      rethrow;
    } catch (_) {
      throw const ApiClientException('Unable to reach Domu. Please try again.');
    }
  }
}

class ApiResponse {
  const ApiResponse(this.statusCode, this.body);

  final int statusCode;
  final String body;

  bool get isSuccess => statusCode >= 200 && statusCode < 300;
}

class ApiClientException implements Exception {
  const ApiClientException(this.message);

  final String message;

  @override
  String toString() => message;
}
