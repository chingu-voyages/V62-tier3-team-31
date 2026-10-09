export type CartItem = {
  productId: string
  title: string
  imageUrl: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
  stockQuantity: number
  available: boolean
}

export type Cart = {
  items: CartItem[]
  itemCount: number
  subtotal: number
}

export const emptyCart: Cart = { items: [], itemCount: 0, subtotal: 0 }
