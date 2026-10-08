// Shapes returned by the backend. They follow the docs/api-contract-*.md files.

export type Category = {
  id: string
  name: string
  slug: string
}

export type Product = {
  id: string
  title: string
  description: string | null
  price: number
  stockQuantity: number
  imageUrl: string | null
  category: Category
}

export type Paged<T> = {
  items: T[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export type User = {
  id: string
  email: string
  firstName: string
  lastName: string
  createdAt: string
}

export type OrderStatus =
  | 'pending'
  | 'paid'
  | 'failed'
  | 'cancelled'
  | 'refunded'
  | 'partially_refunded'
  | 'disputed'

export type FulfillmentStatus =
  | 'unfulfilled'
  | 'processing'
  | 'shipped'
  | 'delivered'
  | 'cancelled'

export type OrderSummary = {
  id: string
  status: OrderStatus
  fulfillmentStatus: FulfillmentStatus
  totalAmount: number
  itemCount: number
  createdAt: string
  paidAt: string | null
}

export type OrderItem = {
  productId: string
  title: string
  imageUrl: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
}

export type ShippingAddress = {
  name: string
  phone: string | null
  line1: string
  line2: string | null
  city: string
  state: string | null
  postalCode: string
  country: string
}

export type Order = {
  id: string
  status: OrderStatus
  fulfillmentStatus: FulfillmentStatus
  totalAmount: number
  refundedAmount: number
  shipping: ShippingAddress
  items: OrderItem[]
  createdAt: string
  paidAt: string | null
}

export type CheckoutRequest = {
  shippingName: string
  shippingPhone: string | null
  shippingLine1: string
  shippingLine2: string | null
  shippingCity: string
  shippingState: string | null
  shippingPostalCode: string
  shippingCountry: string
}

export type CheckoutResponse = {
  orderId: string
  checkoutUrl: string
}
