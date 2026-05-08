import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import * as AuthSession from 'expo-auth-session';
import { router } from 'expo-router';
import * as WebBrowser from 'expo-web-browser';

import {
  getZitadelRedirectUri,
  isZitadelConfigured,
  zitadelClientId,
  zitadelIssuer,
  zitadelScopes,
} from './zitadelAuth';
import { useAuthSession } from './authSession';

WebBrowser.maybeCompleteAuthSession();

export default function ZitadelSignInScreen() {
  const { accessToken, hydrated, setTokenResponse } = useAuthSession();
  const [error, setError] = useState<string | null>(null);
  const [username, setUsername] = useState('');
  const [discovery, setDiscovery] = useState<AuthSession.DiscoveryDocument | null>(null);
  const [discoveryLoading, setDiscoveryLoading] = useState(false);
  const redirectUri = useMemo(() => getZitadelRedirectUri(), []);
  const configured = isZitadelConfigured();
  const loginHint = username.trim();

  useEffect(() => {
    if (hydrated && accessToken) {
      router.replace('/households');
    }
  }, [accessToken, hydrated]);

  useEffect(() => {
    let cancelled = false;

    async function loadDiscovery() {
      if (!zitadelIssuer) {
        setDiscovery(null);
        return;
      }

      setDiscoveryLoading(true);
      setError(null);

      try {
        const document = await AuthSession.fetchDiscoveryAsync(zitadelIssuer);

        if (!cancelled) {
          setDiscovery(document);
        }
      } catch {
        if (!cancelled) {
          setDiscovery(null);
          setError('Could not reach ZITADEL. Check EXPO_PUBLIC_ZITADEL_ISSUER and your network.');
        }
      } finally {
        if (!cancelled) {
          setDiscoveryLoading(false);
        }
      }
    }

    loadDiscovery();

    return () => {
      cancelled = true;
    };
  }, []);

  const [request, , promptAsync] = AuthSession.useAuthRequest(
    {
      clientId: zitadelClientId ?? '',
      redirectUri,
      responseType: AuthSession.ResponseType.Code,
      scopes: zitadelScopes,
      usePKCE: true,
      extraParams: loginHint ? { login_hint: loginHint } : undefined,
    },
    discovery,
  );

  const signIn = useCallback(async () => {
    if (!configured) {
      setError('Set EXPO_PUBLIC_ZITADEL_ISSUER and EXPO_PUBLIC_ZITADEL_CLIENT_ID first.');
      return;
    }

    if (!loginHint) {
      setError('Enter your username first.');
      return;
    }

    if (!request || !discovery) {
      setError('ZITADEL discovery is still loading.');
      return;
    }

    setError(null);

    const result = await promptAsync();

    if (result.type === 'success') {
      try {
        const code = result.params.code;

        if (!code) {
          setError('ZITADEL did not return an authorization code.');
          return;
        }

        const tokenResponse = await AuthSession.exchangeCodeAsync(
          {
            clientId: zitadelClientId ?? '',
            code,
            extraParams: {
              code_verifier: request.codeVerifier ?? '',
            },
            redirectUri,
            scopes: zitadelScopes,
          },
          discovery,
        );

        await setTokenResponse(tokenResponse);
        setError(null);
        router.replace('/households');
        return;
      } catch {
        setError('Could not exchange the ZITADEL code for an access token.');
        return;
      }
    }

    if (result.type === 'error') {
      setError(result.error?.message ?? 'ZITADEL sign-in failed.');
      return;
    }

    if (result.type === 'dismiss' || result.type === 'cancel') {
      setError('Sign-in was cancelled.');
    }
  }, [configured, discovery, loginHint, promptAsync, redirectUri, request, setTokenResponse]);

  const canSignIn = configured && Boolean(request) && Boolean(discovery) && Boolean(loginHint);

  return (
    <KeyboardAvoidingView
      behavior={Platform.select({ ios: 'padding', default: undefined })}
      style={styles.screen}>
      <View style={styles.content}>
        <Text style={styles.eyebrow}>Domu</Text>
        <Text style={styles.title}>Sign in to continue</Text>
        <Text style={styles.body}>
          Enter your username, then continue to ZITADEL to finish authentication.
        </Text>

        <View style={styles.field}>
          <Text style={styles.label}>Username</Text>
          <TextInput
            autoCapitalize="none"
            autoCorrect={false}
            keyboardType="email-address"
            onChangeText={setUsername}
            placeholder="you@example.com"
            placeholderTextColor="#89918a"
            returnKeyType="go"
            style={styles.input}
            textContentType="username"
            value={username}
            onSubmitEditing={signIn}
          />
        </View>

        <Pressable
          accessibilityRole="button"
          disabled={!canSignIn}
          onPress={signIn}
          style={({ pressed }) => [
            styles.button,
            !canSignIn && styles.buttonDisabled,
            pressed && styles.buttonPressed,
          ]}>
          {discoveryLoading ? (
            <ActivityIndicator color="#ffffff" />
          ) : (
            <Text style={styles.buttonText}>Continue</Text>
          )}
        </Pressable>

        <View style={styles.notice}>
          <Text style={styles.noticeTitle}>
            {configured ? 'OAuth redirect URI' : 'Configuration required'}
          </Text>
          {!configured && (
            <Text style={styles.noticeText}>
              Add EXPO_PUBLIC_ZITADEL_ISSUER and EXPO_PUBLIC_ZITADEL_CLIENT_ID to your local
              environment.
            </Text>
          )}
          <Text style={styles.redirectUri}>Redirect URI: {redirectUri}</Text>
        </View>

        {error && <Text style={styles.error}>{error}</Text>}
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#f6f4ef',
    justifyContent: 'center',
    padding: 24,
  },
  content: {
    gap: 18,
  },
  eyebrow: {
    color: '#4f6f52',
    fontSize: 15,
    fontWeight: '700',
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  title: {
    color: '#19201a',
    fontSize: 34,
    fontWeight: '800',
    letterSpacing: 0,
    lineHeight: 40,
  },
  body: {
    color: '#4b544c',
    fontSize: 17,
    lineHeight: 25,
    maxWidth: 360,
  },
  field: {
    gap: 8,
  },
  label: {
    color: '#19201a',
    fontSize: 14,
    fontWeight: '700',
  },
  input: {
    backgroundColor: '#ffffff',
    borderColor: '#cfc8b8',
    borderRadius: 8,
    borderWidth: 1,
    color: '#19201a',
    fontSize: 16,
    minHeight: 52,
    paddingHorizontal: 14,
  },
  button: {
    alignItems: 'center',
    backgroundColor: '#1d5c63',
    borderRadius: 8,
    minHeight: 52,
    justifyContent: 'center',
    marginTop: 10,
    paddingHorizontal: 18,
  },
  buttonDisabled: {
    backgroundColor: '#8ca2a5',
  },
  buttonPressed: {
    opacity: 0.86,
  },
  buttonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '700',
    letterSpacing: 0,
  },
  notice: {
    backgroundColor: '#ffffff',
    borderColor: '#ded9cb',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    padding: 14,
  },
  noticeTitle: {
    color: '#19201a',
    fontSize: 15,
    fontWeight: '700',
  },
  noticeText: {
    color: '#4b544c',
    fontSize: 14,
    lineHeight: 20,
  },
  redirectUri: {
    color: '#5f6760',
    fontSize: 12,
    lineHeight: 18,
  },
  error: {
    color: '#9f2d20',
    fontSize: 14,
    lineHeight: 20,
  },
});
