import * as AuthSession from 'expo-auth-session';
import * as SecureStore from 'expo-secure-store';
import {
  createContext,
  PropsWithChildren,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { zitadelClientId, zitadelIssuer, zitadelScopes } from './zitadelAuth';

const tokenStorageKey = 'domu.auth.tokenResponse';

type StoredTokenResponse = ReturnType<AuthSession.TokenResponse['getRequestConfig']>;

type AuthSessionContextValue = {
  accessToken: string | null;
  clearTokenResponse: () => Promise<void>;
  hydrated: boolean;
  setTokenResponse: (tokenResponse: AuthSession.TokenResponse) => Promise<void>;
  tokenResponse: AuthSession.TokenResponse | null;
};

const AuthSessionContext = createContext<AuthSessionContextValue | null>(null);

export function AuthSessionProvider({ children }: PropsWithChildren) {
  const [tokenResponse, setTokenResponseState] = useState<AuthSession.TokenResponse | null>(null);
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function hydrateTokenResponse() {
      try {
        const storedTokenResponse = await readStoredTokenResponse();

        if (!storedTokenResponse) {
          return;
        }

        const restoredTokenResponse = new AuthSession.TokenResponse(storedTokenResponse);

        if (!restoredTokenResponse.shouldRefresh()) {
          if (!cancelled) {
            setTokenResponseState(restoredTokenResponse);
          }
          return;
        }

        if (!restoredTokenResponse.refreshToken || !zitadelIssuer || !zitadelClientId) {
          await deleteStoredTokenResponse();
          return;
        }

        const discovery = await AuthSession.fetchDiscoveryAsync(zitadelIssuer);
        const refreshedTokenResponse = await restoredTokenResponse.refreshAsync(
          {
            clientId: zitadelClientId,
            scopes: zitadelScopes,
          },
          discovery,
        );

        await writeStoredTokenResponse(refreshedTokenResponse);

        if (!cancelled) {
          setTokenResponseState(refreshedTokenResponse);
        }
      } catch {
        await deleteStoredTokenResponse();
      } finally {
        if (!cancelled) {
          setHydrated(true);
        }
      }
    }

    hydrateTokenResponse();

    return () => {
      cancelled = true;
    };
  }, []);

  const setTokenResponse = useCallback(async (nextTokenResponse: AuthSession.TokenResponse) => {
    setTokenResponseState(nextTokenResponse);
    await writeStoredTokenResponse(nextTokenResponse);
  }, []);

  const clearTokenResponse = useCallback(async () => {
    setTokenResponseState(null);
    await deleteStoredTokenResponse();
  }, []);

  const value = useMemo<AuthSessionContextValue>(
    () => ({
      accessToken: tokenResponse?.accessToken ?? null,
      clearTokenResponse,
      hydrated,
      setTokenResponse,
      tokenResponse,
    }),
    [clearTokenResponse, hydrated, setTokenResponse, tokenResponse],
  );

  return <AuthSessionContext.Provider value={value}>{children}</AuthSessionContext.Provider>;
}

export function useAuthSession() {
  const context = useContext(AuthSessionContext);

  if (!context) {
    throw new Error('useAuthSession must be used within AuthSessionProvider.');
  }

  return context;
}

async function readStoredTokenResponse(): Promise<StoredTokenResponse | null> {
  const storedValue = await SecureStore.getItemAsync(tokenStorageKey);

  if (!storedValue) {
    return null;
  }

  return JSON.parse(storedValue) as StoredTokenResponse;
}

async function writeStoredTokenResponse(tokenResponse: AuthSession.TokenResponse) {
  await SecureStore.setItemAsync(tokenStorageKey, JSON.stringify(tokenResponse.getRequestConfig()));
}

async function deleteStoredTokenResponse() {
  await SecureStore.deleteItemAsync(tokenStorageKey);
}
