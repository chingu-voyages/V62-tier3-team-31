import type { FulfillmentStatus, OrderStatus } from '../types/api'

const statusLabels: Record<OrderStatus, string> = {
  pending: 'Waiting for payment',
  paid: 'Paid',
  failed: 'Payment failed',
  cancelled: 'Cancelled',
  refunded: 'Refunded',
  partially_refunded: 'Partly refunded',
  disputed: 'Under dispute',
}

const fulfillmentLabels: Record<FulfillmentStatus, string> = {
  unfulfilled: 'Not shipped yet',
  processing: 'Getting ready',
  shipped: 'Shipped',
  delivered: 'Delivered',
  cancelled: 'Cancelled',
}

export function statusLabel(status: OrderStatus) {
  return statusLabels[status] ?? status
}

export function fulfillmentLabel(status: FulfillmentStatus) {
  return fulfillmentLabels[status] ?? status
}

// Colour group for the little status pill.
export function statusTone(status: OrderStatus): 'good' | 'warn' | 'bad' {
  if (status === 'paid') return 'good'
  if (status === 'pending' || status === 'partially_refunded') return 'warn'
  return 'bad'
}

export function formatDate(iso: string | null) {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

export function shortId(id: string) {
  return id.slice(0, 8).toUpperCase()
}
