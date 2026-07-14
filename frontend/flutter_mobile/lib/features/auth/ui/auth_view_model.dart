import 'package:flutter/foundation.dart';

import '../domain/auth_repository.dart';
import '../domain/auth_session.dart';

enum AuthSessionState { initializing, unauthenticated, authenticated }

/// Holds authentication UI state and exposes commands for authentication views.
class AuthViewModel extends ChangeNotifier {
  AuthViewModel(this._repository);

  final AuthRepository _repository;
  AuthSessionState _state = AuthSessionState.initializing;
  AuthSession? _session;
  bool _isSigningIn = false;
  String? _signInMessage;

  AuthSessionState get state => _state;
  bool get isSigningIn => _isSigningIn;
  String? get signInMessage => _signInMessage;

  Future<void> initialize() async {
    try {
      _session = await _repository.restoreSession();
      _state = _session == null
          ? AuthSessionState.unauthenticated
          : AuthSessionState.authenticated;
    } catch (_) {
      await _repository.signOut();
      _session = null;
      _state = AuthSessionState.unauthenticated;
    }
    notifyListeners();
  }

  Future<void> signIn(String? loginHint) async {
    if (_isSigningIn) return;
    _isSigningIn = true;
    _signInMessage = null;
    notifyListeners();
    try {
      _session = await _repository.signIn(loginHint: loginHint);
      _state = AuthSessionState.authenticated;
    } on AuthSignInCancelled {
      _signInMessage = 'Sign-in was cancelled.';
    } catch (_) {
      _signInMessage = 'Unable to sign in. Please try again.';
    } finally {
      _isSigningIn = false;
      notifyListeners();
    }
  }

  Future<String?> validAccessToken() async {
    final session = _session;
    if (session == null) return null;
    final updatedSession = await _repository.refreshIfNeeded(session);
    if (updatedSession == null) {
      _session = null;
      _state = AuthSessionState.unauthenticated;
      notifyListeners();
      return null;
    }
    _session = updatedSession;
    return updatedSession.accessToken;
  }

  Future<void> signOut() async {
    await _repository.signOut();
    _session = null;
    _signInMessage = null;
    _state = AuthSessionState.unauthenticated;
    notifyListeners();
  }
}
