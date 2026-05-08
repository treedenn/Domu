import * as AuthSession from 'expo-auth-session';

export const zitadelIssuer = process.env.EXPO_PUBLIC_ZITADEL_ISSUER;
export const zitadelClientId = process.env.EXPO_PUBLIC_ZITADEL_CLIENT_ID;

export const zitadelScopes = ['openid', 'profile', 'email', 'offline_access'];

export function getZitadelRedirectUri() {
  return AuthSession.makeRedirectUri({
    native: 'domu://auth/callback',
    scheme: 'domu',
    path: 'auth/callback',
  });
}

export function isZitadelConfigured() {
  return Boolean(zitadelIssuer && zitadelClientId);
}
