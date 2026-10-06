import { useState } from 'react'
import type { Product } from '../data/products'
import type { CartQuantities } from '../types/cart'

export function useCart() {
  const [cartQuantities, setCartQuantities] = useState<CartQuantities>({})

  const cartCount = Object.values(cartQuantities).reduce<number>(
    (total, quantity) => total + (quantity ?? 0),
    0,
  )

  const addToCart = (id: Product['id']) => {
    setCartQuantities((current) => ({
      ...current,
      [id]: (current[id] ?? 0) + 1,
    }))
  }

  const changeQuantity = (id: Product['id'], delta: number) => {
    setCartQuantities((current) => {
      const quantity = (current[id] ?? 0) + delta

      if (quantity <= 0) {
        const next = { ...current }
        delete next[id]
        return next
      }

      return { ...current, [id]: quantity }
    })
  }

  const removeFromCart = (id: Product['id']) => {
    setCartQuantities((current) => {
      const next = { ...current }
      delete next[id]
      return next
    })
  }

  return {
    cartQuantities,
    cartCount,
    addToCart,
    changeQuantity,
    removeFromCart,
  }
}
