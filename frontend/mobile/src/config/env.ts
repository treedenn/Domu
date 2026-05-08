const trimTrailingSlashes = (value: string) => value.replace(/\/+$/, '');

export const apiUrl = trimTrailingSlashes(
  process.env.EXPO_PUBLIC_API_URL?.trim() || 'http://localhost:5070',
);

export const apiV1Url = `${apiUrl}/api/v1`;

