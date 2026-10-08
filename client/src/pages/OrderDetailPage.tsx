import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { PageShell } from '../components/PageShell'
import { money } from '../data/products'
import { ApiError, api, isAbortError } from '../lib/api'
import { fulfillmentLabel, formatDate, shortId, statusLabel, statusTone } from '../lib/format'
import type { Order } from '../types/api'

export function OrderDetailPage() {
  const { id = '' } = useParams()
  const [order, setOrder] = useState<Order | null>(null)
  const [problem, setProblem] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    api<Order>(`/orders/${id}`, { signal: controller.signal })
      .then((data) => {
        setOrder(data)
        setProblem('')
      })
      .catch((error: unknown) => {
        if (isAbortError(error)) return
        const missing = error instanceof ApiError && (error.status === 404 || error.status === 400)
        setProblem(missing ? 'We could not find that order.' : 'We could not load this order. Please refresh the page.')
      })

    return () => controller.abort()
  }, [id])

  return (
    <PageShell>
      <section className="form-card wide-card">
        <Link className="text-link back-link" to="/orders">
          ← All orders
        </Link>

        {problem && <p className="form-error">{problem}</p>}
        {!order && !problem && <p className="page-note">Loading…</p>}

        {order && (
          <>
            <div className="order-head">
              <div>
                <p className="section-kicker">Order</p>
                <h1>#{shortId(order.id)}</h1>
                <p className="page-note">
                  {order.paidAt
                    ? `Paid on ${formatDate(order.paidAt)}`
                    : `Placed on ${formatDate(order.createdAt)}`}
                </p>
              </div>
              <div className="order-statuses">
                <span className={`status-pill ${statusTone(order.status)}`}>{statusLabel(order.status)}</span>
                {order.status !== 'pending' && order.status !== 'failed' && (
                  <span className="status-pill neutral">{fulfillmentLabel(order.fulfillmentStatus)}</span>
                )}
              </div>
            </div>

            <ul className="summary-list">
              {order.items.map((item) => (
                <li key={item.productId}>
                  <span>
                    {item.title} <small>× {item.quantity}</small>
                    <small className="muted"> · {money.format(item.unitPrice)} each</small>
                  </span>
                  <strong>{money.format(item.lineTotal)}</strong>
                </li>
              ))}
            </ul>

            <div className="subtotal-line">
              <span>Total</span>
              <strong>{money.format(order.totalAmount)}</strong>
            </div>
            {order.refundedAmount > 0 && (
              <div className="subtotal-line refund-line">
                <span>Refunded</span>
                <strong>−{money.format(order.refundedAmount)}</strong>
              </div>
            )}

            <h2 className="detail-heading">Delivery address</h2>
            <address className="address-block">
              {order.shipping.name}
              <br />
              {order.shipping.line1}
              <br />
              {order.shipping.line2 && (
                <>
                  {order.shipping.line2}
                  <br />
                </>
              )}
              {order.shipping.city}
              {order.shipping.state ? `, ${order.shipping.state}` : ''} {order.shipping.postalCode}
              <br />
              {order.shipping.country}
              {order.shipping.phone && (
                <>
                  <br />
                  {order.shipping.phone}
                </>
              )}
            </address>
          </>
        )}
      </section>
    </PageShell>
  )
}
