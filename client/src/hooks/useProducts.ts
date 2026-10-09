import { useCallback, useEffect, useState } from 'react'
import { api, isAbortError } from '../lib/api'
import type { Paged, Product } from '../types/api'

type Status = 'loading' | 'ready' | 'error'

// The store is small, so one request for the first 50 products is enough.
// Search and category filtering then happen on the page (see useProductFilters).
export function useProducts() {
  const [products, setProducts] = useState<Product[]>([])
  const [status, setStatus] = useState<Status>('loading')
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    api<Paged<Product>>('/products?limit=50', { signal: controller.signal })
      .then((result) => {
        setProducts(result.items)
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) setStatus('error')
      })

    return () => controller.abort()
  }, [attempt])

  const retry = useCallback(() => {
    setStatus('loading')
    setAttempt((count) => count + 1)
  }, [])

  return { products, status, retry }
}
