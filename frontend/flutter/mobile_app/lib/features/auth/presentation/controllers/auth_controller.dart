import 'package:flutter/foundation.dart';
import 'package:flutter_appauth/flutter_appauth.dart';

import '../../../../core/auth/auth_session.dart';
import '../../data/auth_repository.dart';
import '../../data/zitadel_auth_repository.dart';

class AuthController extends ChangeNotifier {
  AuthController(this._repository);

  final AuthRepository _repository;

  AuthState _state = const AuthState.initializing();

  AuthState get state => _state;

  Future<void> restoreSession() async {
    _state = const AuthState.initializing();
    notifyListeners();

    try {
      final AuthSession? session = await _repository.restoreSession();
      _state = AuthState(
        session: session,
        isInitializing: false,
      );
    } catch (error) {
      _state = AuthState.unauthenticated(
        errorMessage: _formatError(error),
      );
    }

    notifyListeners();
  }

  Future<void> signIn({
    String? loginHint,
    String? preferredIdpId,
    bool createAccount = false,
  }) async {
    _state = _state.copyWith(
      isBusy: true,
      errorMessage: null,
    );
    notifyListeners();

    try {
      final AuthSession session = await _repository.signIn(
        loginHint: loginHint,
        preferredIdpId: preferredIdpId,
        createAccount: createAccount,
      );

      _state = AuthState.authenticated(session);
    } on FlutterAppAuthUserCancelledException {
      _state = _state.copyWith(isBusy: false);
    } catch (error) {
      _state = AuthState.unauthenticated(
        errorMessage: _formatError(error),
      );
    }

    notifyListeners();
  }

  Future<void> signOut() async {
    final AuthSession? existingSession = _state.session;

    _state = _state.copyWith(
      isBusy: true,
      errorMessage: null,
    );
    notifyListeners();

    try {
      await _repository.signOut(existingSession);
      _state = const AuthState.unauthenticated();
    } catch (error) {
      _state = AuthState.unauthenticated(
        errorMessage: _formatError(error),
      );
    }

    notifyListeners();
  }

  String _formatError(Object error) {
    if (error is AuthRepositoryException) {
      return error.message;
    }

    if (error is FlutterAppAuthPlatformException) {
      final String details = error.details.toString();
      return details.isEmpty ? (error.message ?? 'Authentication failed.') : details;
    }

    return error.toString();
  }
}

class AuthState {
  const AuthState({
    required this.session,
    required this.isInitializing,
    this.isBusy = false,
    this.errorMessage,
  });

  const AuthState.initializing()
      : session = null,
        isInitializing = true,
        isBusy = false,
        errorMessage = null;

  const AuthState.unauthenticated({this.errorMessage})
      : session = null,
        isInitializing = false,
        isBusy = false;

  const AuthState.authenticated(AuthSession this.session)
      : isInitializing = false,
        isBusy = false,
        errorMessage = null;

  final AuthSession? session;
  final bool isInitializing;
  final bool isBusy;
  final String? errorMessage;

  bool get isAuthenticated => session != null;

  AuthState copyWith({
    AuthSession? session,
    bool? isInitializing,
    bool? isBusy,
    String? errorMessage,
    bool clearError = false,
  }) {
    return AuthState(
      session: session ?? this.session,
      isInitializing: isInitializing ?? this.isInitializing,
      isBusy: isBusy ?? this.isBusy,
      errorMessage: clearError ? null : errorMessage ?? this.errorMessage,
    );
  }
}
