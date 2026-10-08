import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { PageShell } from '../components/PageShell'
import { money } from '../data/products'
import { useCart } from '../hooks/useCart'
import { ApiError, api } from '../lib/api'
import { shortId } from '../lib/format'
import type { Order } from '../types/api'

const POLL_EVERY_MS = 2000
const GIVE_UP_AFTER = 30 // 30 tries x 2 seconds = about a minute

const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

// Stripe sends the customer back here. The webhook is what actually marks the order paid,
// and it can land a second or two later, so this page asks until the status leaves "pending".
export function CheckoutSuccessPage() {
  const [params] = useSearchParams()
  const orderId = params.get('orderId') ?? ''
  const validId = uuidPattern.test(orderId)
  const { refreshCart } = useCart()

  const [order, setOrder] = useState<Order | null>(null)
  const [notFound, setNotFound] = useState(false)
  const [gaveUp, setGaveUp] = useState(false)

  useEffect(() => {
    if (!validId) return

    let cancelled = false
    let timer: ReturnType<typeof setTimeout> | undefined
    let tries = 0

    const check = async () => {
      tries += 1
      try {
        const result = await api<Order>(`/orders/${orderId}`)
        if (cancelled) return
        setOrder(result)

        if (result.status !== 'pending') {
          // The webhook emptied the cart on the server, so load the empty cart.
          void refreshCart()
          return
        }
      } catch (error) {
        if (cancelled) return
        if (error instanceof ApiError && error.status === 404) {
          setNotFound(true)
          return
        }
        // Any other error: try again on the next round.
      }

      if (tries >= GIVE_UP_AFTER) {
        setGaveUp(true)
        return
      }
      timer = setTimeout(check, POLL_EVERY_MS)
    }

    void check()

    return () => {
      cancelled = true
      if (timer) clearTimeout(timer)
    }
  }, [orderId, validId, refreshCart])

  if (!validId || notFound) {
    return (
      <PageShell>
        <section className="form-card result-card">
          <h1>We could not find that order</h1>
          <p className="page-note">Check “My orders” to see everything you have bought.</p>
          <Link className="btn btn-primary" to="/orders">
            My orders
          </Link>
        </section>
      </PageShell>
    )
  }

  if (!order || order.status === 'pending') {
    if (gaveUp) {
      return (
        <PageShell>
          <section className="form-card result-card">
            <h1>Still confirming your payment</h1>
            <p className="page-note">
              This is taking longer than usual. You do not need to pay again. Your order will appear in
              “My orders” as soon as the payment is confirmed.
            </p>
            <Link className="btn btn-primary" to="/orders">
              My orders
            </Link>
          </section>
        </PageShell>
      )
    }

    return (
      <PageShell>
        <section className="form-card result-card" aria-live="polite">
          <div className="spinner" aria-hidden="true" />
          <h1>Confirming your payment…</h1>
          <p className="page-note">Please keep this page open for a moment.</p>
        </section>
      </PageShell>
    )
  }

  if (order.status === 'failed' || order.status === 'cancelled') {
    return (
      <PageShell>
        <section className="form-card result-card">
          <h1>The payment did not go through</h1>
          <p className="page-note">Nothing was charged. Your cart is still saved, so you can try again.</p>
          <Link className="btn btn-primary" to="/checkout">
            Try again
          </Link>
        </section>
      </PageShell>
    )
  }

  return (
    <PageShell>
      <section className="form-card result-card">
        <div className="tick" aria-hidden="true">
          ✓
        </div>
        <h1>Thank you, your order is confirmed</h1>
        <p className="page-note">
          Order <strong>#{shortId(order.id)}</strong> · {money.format(order.totalAmount)}
        </p>
        <ul className="summary-list">
          {order.items.map((item) => (
            <li key={item.productId}>
              <span>
                {item.title} <small>× {item.quantity}</small>
              </span>
              <strong>{money.format(item.lineTotal)}</strong>
            </li>
          ))}
        </ul>
        <div className="button-row">
          <Link className="btn btn-primary" to={`/orders/${order.id}`}>
            View order
          </Link>
          <Link className="btn btn-secondary" to="/">
            Keep shopping
          </Link>
        </div>
      </section>
    </PageShell>
  )
}
