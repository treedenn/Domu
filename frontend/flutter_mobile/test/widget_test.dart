import 'package:domu_mobile/features/auth/domain/auth_repository.dart';
import 'package:domu_mobile/features/auth/domain/auth_session.dart';
import 'package:domu_mobile/features/auth/ui/auth_view_model.dart';
import 'package:domu_mobile/features/auth/ui/login_view.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('shows the Zitadel login page', (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(home: LoginView(viewModel: AuthViewModel(_NoOpRepository()))),
    );

    expect(find.text('Welcome home'), findsOneWidget);
    expect(find.text('Continue with Zitadel'), findsOneWidget);
    expect(find.byType(TextField), findsOneWidget);
  });
}

class _NoOpRepository implements AuthRepository {
  @override
  Future<AuthSession?> refreshIfNeeded(AuthSession session) async => session;

  @override
  Future<AuthSession?> restoreSession() async => null;

  @override
  Future<AuthSession> signIn({String? loginHint}) => throw UnimplementedError();

  @override
  Future<void> signOut() async {}
}
