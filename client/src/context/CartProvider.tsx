import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useAuth } from '../hooks/useAuth'
import { ApiError, api } from '../lib/api'
import { emptyCart, type Cart } from '../types/cart'
import { CartContext, type CartContextValue } from './cart-context'

export function CartProvider({ children }: { children: ReactNode }) {
  const { user, loading: authLoading } = useAuth()
  const [cart, setCart] = useState<Cart>(emptyCart)
  const [message, setMessage] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  // Only the newest response is allowed to replace the cart, so quick clicks can't
  // leave an older answer on screen.
  const latestRequest = useRef(0)

  const run = useCallback(async (request: () => Promise<Cart>) => {
    const requestId = ++latestRequest.current
    try {
      const next = await request()
      if (requestId === latestRequest.current) {
        setCart(next)
        setMessage('')
      }
      return true
    } catch (error) {
      if (requestId === latestRequest.current) {
        setMessage(
          error instanceof ApiError ? error.message : 'Could not update your cart. Please try again.',
        )
      }
      return false
    }
  }, [])

  const refreshCart = useCallback(async () => {
    await run(() => api<Cart>('/cart'))
  }, [run])

  // Load the cart once we know who the visitor is, and again when that changes.
  // Logging in merges the guest cart on the server, so this shows the merged result.
  // Waiting for auth first avoids creating a stray guest cart for someone who is logged in.
  const userId = user?.id
  useEffect(() => {
    if (authLoading) return
    void refreshCart()
  }, [authLoading, userId, refreshCart])

  const addItem = useCallback(
    (productId: string, quantity = 1) =>
      run(() => api<Cart>('/cart/items', { method: 'POST', body: { productId, quantity } })),
    [run],
  )

  const setQuantity = useCallback(
    (productId: string, quantity: number) =>
      run(() => api<Cart>(`/cart/items/${productId}`, { method: 'PATCH', body: { quantity } })),
    [run],
  )

  const removeItem = useCallback(
    (productId: string) => run(() => api<Cart>(`/cart/items/${productId}`, { method: 'DELETE' })),
    [run],
  )

  const openCart = useCallback(() => setIsOpen(true), [])
  const closeCart = useCallback(() => setIsOpen(false), [])
  const clearMessage = useCallback(() => setMessage(''), [])

  const value = useMemo<CartContextValue>(
    () => ({
      cart,
      message,
      isOpen,
      openCart,
      closeCart,
      clearMessage,
      refreshCart,
      addItem,
      setQuantity,
      removeItem,
    }),
    [cart, message, isOpen, openCart, closeCart, clearMessage, refreshCart, addItem, setQuantity, removeItem],
  )

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}
