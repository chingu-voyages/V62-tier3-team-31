import { createContext } from 'react'
import type { Cart } from '../types/cart'

export type CartContextValue = {
  cart: Cart
  // a sentence to show when the last cart action failed, empty when everything is fine
  message: string
  isOpen: boolean
  openCart: () => void
  closeCart: () => void
  clearMessage: () => void
  refreshCart: () => Promise<void>
  addItem: (productId: string, quantity?: number) => Promise<boolean>
  setQuantity: (productId: string, quantity: number) => Promise<boolean>
  removeItem: (productId: string) => Promise<boolean>
}

export const CartContext = createContext<CartContextValue | null>(null)
