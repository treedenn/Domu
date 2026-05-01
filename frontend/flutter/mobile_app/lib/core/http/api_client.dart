import 'dart:convert';
import 'dart:io';

import '../auth/auth_session.dart';
import 'api_error.dart';

class ApiClient {
  const ApiClient({required this.baseUrl});

  final String baseUrl;

  Future<Object?> getJson(
    String path, {
    required AuthSession session,
    Map<String, String?> queryParameters = const <String, String?>{},
  }) async {
    final Uri uri = _uri(path, queryParameters);
    final HttpClient client = HttpClient();

    try {
      final HttpClientRequest request = await client.getUrl(uri);
      request.headers.set(HttpHeaders.acceptHeader, 'application/json');
      request.headers.set(
        HttpHeaders.authorizationHeader,
        '${session.tokenType ?? 'Bearer'} ${session.accessToken}',
      );

      final HttpClientResponse response = await request.close();
      final String body = await response.transform(utf8.decoder).join();

      if (response.statusCode < 200 || response.statusCode >= 300) {
        throw ApiError(
          _extractErrorMessage(body) ?? 'Request failed.',
          statusCode: response.statusCode,
        );
      }

      if (body.trim().isEmpty) {
        return null;
      }

      return jsonDecode(body);
    } on SocketException catch (error) {
      throw ApiError('Unable to reach the API. ${error.message}');
    } on FormatException {
      throw const ApiError('The API returned invalid JSON.');
    } finally {
      client.close(force: true);
    }
  }

  Future<Object?> postJson(
    String path, {
    required AuthSession session,
    required Map<String, Object?> body,
  }) {
    return _sendJson('POST', path, session: session, body: body);
  }

  Future<Object?> putJson(
    String path, {
    required AuthSession session,
    required Map<String, Object?> body,
  }) {
    return _sendJson('PUT', path, session: session, body: body);
  }

  Future<void> deleteJson(
    String path, {
    required AuthSession session,
  }) async {
    await _sendJson('DELETE', path, session: session);
  }

  Future<Object?> _sendJson(
    String method,
    String path, {
    required AuthSession session,
    Map<String, Object?>? body,
  }) async {
    final Uri uri = _uri(path, const <String, String?>{});
    final HttpClient client = HttpClient();

    try {
      final HttpClientRequest request = await client.openUrl(method, uri);
      request.headers.set(HttpHeaders.acceptHeader, 'application/json');
      request.headers.set(HttpHeaders.contentTypeHeader, 'application/json');
      request.headers.set(
        HttpHeaders.authorizationHeader,
        '${session.tokenType ?? 'Bearer'} ${session.accessToken}',
      );
      if (body != null) {
        request.write(jsonEncode(body));
      }

      final HttpClientResponse response = await request.close();
      final String responseBody = await response.transform(utf8.decoder).join();

      if (response.statusCode < 200 || response.statusCode >= 300) {
        throw ApiError(
          _extractErrorMessage(responseBody) ?? 'Request failed.',
          statusCode: response.statusCode,
        );
      }

      if (responseBody.trim().isEmpty) {
        return null;
      }

      return jsonDecode(responseBody);
    } on SocketException catch (error) {
      throw ApiError('Unable to reach the API. ${error.message}');
    } on FormatException {
      throw const ApiError('The API returned invalid JSON.');
    } finally {
      client.close(force: true);
    }
  }

  Uri _uri(String path, Map<String, String?> queryParameters) {
    final String normalizedBaseUrl = baseUrl.endsWith('/')
        ? baseUrl.substring(0, baseUrl.length - 1)
        : baseUrl;
    final String normalizedPath = path.startsWith('/') ? path : '/$path';
    final Uri uri = Uri.parse('$normalizedBaseUrl$normalizedPath');
    final Map<String, String> cleanedQueryParameters = <String, String>{
      for (final MapEntry<String, String?> entry in queryParameters.entries)
        if (entry.value != null) entry.key: entry.value!,
    };

    if (cleanedQueryParameters.isEmpty) {
      return uri;
    }

    return uri.replace(queryParameters: <String, String>{
      ...uri.queryParameters,
      ...cleanedQueryParameters,
    });
  }

  String? _extractErrorMessage(String body) {
    if (body.trim().isEmpty) {
      return null;
    }

    try {
      final Object? json = jsonDecode(body);
      if (json is Map<String, Object?>) {
        return json['detail'] as String? ??
            json['title'] as String? ??
            json['message'] as String?;
      }
    } catch (_) {
      return body;
    }

    return body;
  }
}
