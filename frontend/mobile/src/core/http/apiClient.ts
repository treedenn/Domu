import { apiV1Url } from '@/config/env';
import { TimeoutError } from '@/core/async/timeout';

export type ApiQueryValue = string | number | boolean | null | undefined;

export type ApiRequestOptions = Omit<RequestInit, 'body'> & {
  accessToken?: string | null;
  body?: unknown;
  query?: Record<string, ApiQueryValue>;
};

export type ProblemDetails = {
  detail?: string;
  errors?: Record<string, string[]>;
  status?: number;
  title?: string;
  type?: string;
};

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly problem?: ProblemDetails,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

const defaultRequestTimeoutMs = 15000;

export async function apiRequest<TResponse>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<TResponse> {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), defaultRequestTimeoutMs);

  let response: Response;

  try {
    response = await fetch(buildUrl(path, options.query), {
      ...buildInit(options),
      signal: options.signal ?? controller.signal,
    });
  } catch (exception) {
    if (isAbortError(exception)) {
      throw new TimeoutError(
        `The API did not respond within ${defaultRequestTimeoutMs / 1000} seconds.`,
      );
    }

    throw exception;
  } finally {
    clearTimeout(timeoutId);
  }

  if (!response.ok) {
    const problem = await readProblemDetails(response);
    throw new ApiError(
      problem?.detail || problem?.title || `Request failed with status ${response.status}.`,
      response.status,
      problem,
    );
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

function isAbortError(exception: unknown) {
  return (
    typeof exception === 'object' &&
    exception !== null &&
    'name' in exception &&
    exception.name === 'AbortError'
  );
}

function buildUrl(path: string, query?: Record<string, ApiQueryValue>) {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  const url = new URL(`${apiV1Url}${normalizedPath}`);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined) {
        url.searchParams.set(key, String(value));
      }
    }
  }

  return url.toString();
}

function buildInit(options: ApiRequestOptions): RequestInit {
  const { accessToken, body, headers, query: _query, ...init } = options;
  const requestHeaders = new Headers(headers);

  if (accessToken) {
    requestHeaders.set('Authorization', `Bearer ${accessToken}`);
  }

  if (body !== undefined && !requestHeaders.has('Content-Type')) {
    requestHeaders.set('Content-Type', 'application/json');
  }

  return {
    ...init,
    body: body === undefined ? undefined : JSON.stringify(body),
    headers: requestHeaders,
  };
}

async function readProblemDetails(response: Response): Promise<ProblemDetails | undefined> {
  const contentType = response.headers.get('Content-Type') ?? '';

  if (!contentType.includes('application/json')) {
    return undefined;
  }

  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return undefined;
  }
}

