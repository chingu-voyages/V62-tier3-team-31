import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { AUTH_EXPIRED_EVENT, api } from '../lib/api'
import type { User } from '../types/api'
import { AuthContext, type AuthContextValue, type RegisterInput } from './auth-context'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  // Ask the backend who we are when the app opens. A 401 is normal for a new visitor.
  useEffect(() => {
    let cancelled = false

    api<{ user: User }>('/auth/me')
      .then((result) => {
        if (!cancelled) setUser(result.user)
      })
      .catch(() => {
        if (!cancelled) setUser(null)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  // api.ts fires this when a refresh fails in the middle of a session.
  useEffect(() => {
    const dropUser = () => setUser(null)
    window.addEventListener(AUTH_EXPIRED_EVENT, dropUser)
    return () => window.removeEventListener(AUTH_EXPIRED_EVENT, dropUser)
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const result = await api<{ user: User }>('/auth/login', {
      method: 'POST',
      body: { email, password },
    })
    setUser(result.user)
  }, [])

  const register = useCallback(async (input: RegisterInput) => {
    const result = await api<{ user: User }>('/auth/register', {
      method: 'POST',
      body: input,
    })
    setUser(result.user)
  }, [])

  const logout = useCallback(async () => {
    try {
      await api('/auth/logout', { method: 'POST' })
    } catch {
      // Logging out should always look like it worked on screen.
    }
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({ user, loading, login, register, logout }),
    [user, loading, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
