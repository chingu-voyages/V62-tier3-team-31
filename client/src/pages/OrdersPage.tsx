import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { PageShell } from '../components/PageShell'
import { money } from '../data/products'
import { api, isAbortError } from '../lib/api'
import { formatDate, shortId, statusLabel, statusTone } from '../lib/format'
import type { OrderSummary, Paged } from '../types/api'

const PAGE_SIZE = 10

export function OrdersPage() {
  const [page, setPage] = useState(1)
  const [result, setResult] = useState<Paged<OrderSummary> | null>(null)
  const [failed, setFailed] = useState(false)

  useEffect(() => {
    const controller = new AbortController()

    api<Paged<OrderSummary>>(`/orders?page=${page}&limit=${PAGE_SIZE}`, { signal: controller.signal })
      .then((data) => {
        setResult(data)
        setFailed(false)
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) setFailed(true)
      })

    return () => controller.abort()
  }, [page])

  return (
    <PageShell>
      <section className="form-card wide-card">
        <p className="section-kicker">Your account</p>
        <h1>My orders</h1>

        {failed && <p className="form-error">We could not load your orders. Please refresh the page.</p>}
        {!result && !failed && <p className="page-note">Loading…</p>}

        {result && result.items.length === 0 && (
          <div className="empty-state">
            <p className="page-note">You have not placed an order yet.</p>
            <Link className="btn btn-primary" to="/">
              Start shopping
            </Link>
          </div>
        )}

        {result && result.items.length > 0 && (
          <ul className="order-list">
            {result.items.map((order) => (
              <li key={order.id}>
                <Link className="order-row" to={`/orders/${order.id}`}>
                  <span className="order-main">
                    <strong>Order #{shortId(order.id)}</strong>
                    <small>
                      {formatDate(order.paidAt ?? order.createdAt)} · {order.itemCount}{' '}
                      {order.itemCount === 1 ? 'item' : 'items'}
                    </small>
                  </span>
                  <span className={`status-pill ${statusTone(order.status)}`}>
                    {statusLabel(order.status)}
                  </span>
                  <strong className="order-total">{money.format(order.totalAmount)}</strong>
                </Link>
              </li>
            ))}
          </ul>
        )}

        {result && result.totalPages > 1 && (
          <div className="pager">
            <button
              className="btn btn-secondary"
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((current) => current - 1)}
            >
              Newer
            </button>
            <span>
              Page {result.page} of {result.totalPages}
            </span>
            <button
              className="btn btn-secondary"
              type="button"
              disabled={page >= result.totalPages}
              onClick={() => setPage((current) => current + 1)}
            >
              Older
            </button>
          </div>
        )}
      </section>
    </PageShell>
  )
}
