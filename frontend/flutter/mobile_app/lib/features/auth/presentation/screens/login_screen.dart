import 'package:flutter/material.dart';

import '../../../../app/theme/tokens.dart';
import '../../../../app/bootstrap/app_config.dart';
import '../controllers/auth_controller.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({
    required this.controller,
    required this.config,
    super.key,
  });

  final AuthController controller;
  final AppConfig config;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  late final TextEditingController _emailController;

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController();
  }

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authState = widget.controller.state;
    final bool isBusy = authState.isBusy;

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(AppSpacing.xl),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Card(
                child: Padding(
                  padding: const EdgeInsets.all(AppSpacing.xl),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: <Widget>[
                      Text(
                        'Sign in to Domu',
                        style: Theme.of(context).textTheme.headlineMedium,
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Continue with SSO or send the user through the hosted ZITADEL sign-in flow.',
                        style: Theme.of(context).textTheme.bodyLarge,
                      ),
                      const SizedBox(height: 24),
                      FilledButton.icon(
                        onPressed: isBusy || !widget.config.hasGoogleSso
                            ? null
                            : () => widget.controller.signIn(
                                  preferredIdpId: widget.config.googleIdpId,
                                ),
                        icon: const Icon(Icons.account_circle_outlined),
                        label: const Text('Continue with Google'),
                      ),
                      const SizedBox(height: 12),
                      FilledButton.icon(
                        style: FilledButton.styleFrom(
                          backgroundColor:
                              Theme.of(context).colorScheme.secondary,
                          foregroundColor:
                              Theme.of(context).colorScheme.onSecondary,
                        ),
                        onPressed: isBusy || !widget.config.hasFacebookSso
                            ? null
                            : () => widget.controller.signIn(
                                  preferredIdpId: widget.config.facebookIdpId,
                                ),
                        icon: const Icon(Icons.groups_2_outlined),
                        label: const Text('Continue with Facebook'),
                      ),
                      if (!widget.config.hasGoogleSso || !widget.config.hasFacebookSso) ...<Widget>[
                        const SizedBox(height: 12),
                        Text(
                          'Configure OIDC_GOOGLE_IDP_ID and OIDC_FACEBOOK_IDP_ID to enable provider-specific SSO shortcuts.',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ],
                      const SizedBox(height: 24),
                      const _DividerLabel(label: 'or use your email'),
                      const SizedBox(height: 24),
                      TextField(
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        autofillHints: const <String>[AutofillHints.username],
                        decoration: const InputDecoration(
                          labelText: 'Email',
                          hintText: 'you@example.com',
                          border: OutlineInputBorder(),
                        ),
                      ),
                      const SizedBox(height: 12),
                      OutlinedButton(
                        onPressed: isBusy
                            ? null
                            : () => widget.controller.signIn(
                                  loginHint: _emailController.text,
                                ),
                        child: const Text('Sign in with email/password'),
                      ),
                      const SizedBox(height: 12),
                      TextButton(
                        onPressed: isBusy
                            ? null
                            : () => widget.controller.signIn(
                                  loginHint: _emailController.text,
                                  createAccount: true,
                                ),
                        child: const Text('Create account'),
                      ),
                      if (authState.errorMessage != null) ...<Widget>[
                        const SizedBox(height: 16),
                        Text(
                          authState.errorMessage!,
                          style: TextStyle(
                            color: Theme.of(context).colorScheme.error,
                          ),
                        ),
                      ],
                      if (isBusy) ...<Widget>[
                        const SizedBox(height: 16),
                        const Center(child: CircularProgressIndicator()),
                      ],
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _DividerLabel extends StatelessWidget {
  const _DividerLabel({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        const Expanded(child: Divider()),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12),
          child: Text(
            label,
            style: Theme.of(context).textTheme.labelMedium,
          ),
        ),
        const Expanded(child: Divider()),
      ],
    );
  }
}
