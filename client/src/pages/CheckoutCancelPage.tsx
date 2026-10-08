import { Link } from 'react-router-dom'
import { PageShell } from '../components/PageShell'

// Stripe sends the customer here if they close the payment page without paying.
export function CheckoutCancelPage() {
  return (
    <PageShell>
      <section className="form-card result-card">
        <h1>Payment cancelled</h1>
        <p className="page-note">Nothing was charged. Your cart is still saved.</p>
        <div className="button-row">
          <Link className="btn btn-primary" to="/checkout">
            Back to checkout
          </Link>
          <Link className="btn btn-secondary" to="/">
            Keep shopping
          </Link>
        </div>
      </section>
    </PageShell>
  )
}
