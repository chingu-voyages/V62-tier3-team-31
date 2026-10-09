// One place that talks to the backend.
//
// Every call uses a relative URL (/api/v1/...). In development Vite forwards /api to the
// backend (see vite.config.ts). In production Vercel forwards it (see vercel.json).
// Because the browser only ever talks to its own site, the auth cookies are first-party
// and there is no CORS to configure.

const BASE = import.meta.env.VITE_API_BASE ?? '/api/v1'

export const AUTH_EXPIRED_EVENT = 'auth:expired'

type ErrorBody = {
  title?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  status: number
  errors: Record<string, string[]>

  constructor(status: number, title: string, errors: Record<string, string[]> = {}) {
    // For a 400 the useful sentence is the first field error, not "One or more validation errors".
    const firstFieldError = Object.values(errors).flat()[0]
    super(firstFieldError ?? title)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PATCH' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

// Paths that must never trigger a refresh and retry.
const NO_REFRESH = ['/auth/login', '/auth/register', '/auth/refresh']

let refreshInFlight: Promise<boolean> | null = null

// Several requests can fail with 401 at the same moment. They all share one refresh call.
function refreshSession(): Promise<boolean> {
  if (!refreshInFlight) {
    refreshInFlight = fetch(`${BASE}/auth/refresh`, { method: 'POST', credentials: 'include' })
      .then((response) => response.ok)
      .catch(() => false)
      .finally(() => {
        refreshInFlight = null
      })
  }
  return refreshInFlight
}

function send(path: string, options: RequestOptions) {
  const headers: Record<string, string> = {}
  if (options.body !== undefined) headers['Content-Type'] = 'application/json'

  return fetch(`${BASE}${path}`, {
    method: options.method ?? 'GET',
    credentials: 'include',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    signal: options.signal,
  })
}

export async function api<T>(path: string, options: RequestOptions = {}): Promise<T> {
  let response = await send(path, options)

  if (response.status === 401 && !NO_REFRESH.some((prefix) => path.startsWith(prefix))) {
    const refreshed = await refreshSession()
    if (refreshed) {
      response = await send(path, options)
    } else if (path !== '/auth/me') {
      // The session is really over. Let the app drop the user and send them to login.
      window.dispatchEvent(new Event(AUTH_EXPIRED_EVENT))
    }
  }

  if (response.status === 204) return undefined as T

  const data = (await response.json().catch(() => null)) as (ErrorBody & T) | null

  if (!response.ok) {
    throw new ApiError(response.status, data?.title ?? 'Something went wrong', data?.errors)
  }

  return data as T
}

export function isAbortError(error: unknown) {
  return error instanceof DOMException && error.name === 'AbortError'
}
