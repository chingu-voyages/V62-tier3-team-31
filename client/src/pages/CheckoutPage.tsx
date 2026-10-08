import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Field } from '../components/Field'
import { PageShell } from '../components/PageShell'
import { money } from '../data/products'
import { useAuth } from '../hooks/useAuth'
import { useCart } from '../hooks/useCart'
import { useCheckout, type ShippingForm } from '../hooks/useCheckout'

export function CheckoutPage() {
  const { user } = useAuth()
  const { cart } = useCart()
  const { submitting, fieldErrors, formError, submit } = useCheckout()

  const [form, setForm] = useState<ShippingForm>({
    name: user ? `${user.firstName} ${user.lastName}` : '',
    phone: '',
    line1: '',
    line2: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
  })

  const set = (key: keyof ShippingForm) => (value: string) =>
    setForm((current) => ({ ...current, [key]: value }))

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void submit(form)
  }

  const isEmpty = cart.items.length === 0
  const hasUnavailable = cart.items.some((item) => !item.available)

  if (isEmpty) {
    return (
      <PageShell>
        <section className="form-card">
          <h1>Your cart is empty</h1>
          <p className="page-note">Add something to your cart before checking out.</p>
          <Link className="btn btn-primary" to="/">
            Back to the shop
          </Link>
        </section>
      </PageShell>
    )
  }

  return (
    <PageShell>
      <div className="checkout-layout">
        <section className="form-card">
          <p className="section-kicker">Step 1 of 2</p>
          <h1>Delivery details</h1>
          <p className="page-note">Who should we send the order to? You pay on the next page, on Stripe.</p>

          <form noValidate onSubmit={handleSubmit}>
            <Field
              id="name"
              label="Full name"
              autoComplete="name"
              value={form.name}
              onChange={set('name')}
              error={fieldErrors.name}
            />
            <Field
              id="phone"
              label="Phone (optional)"
              type="tel"
              autoComplete="tel"
              value={form.phone}
              onChange={set('phone')}
              error={fieldErrors.phone}
            />
            <Field
              id="line1"
              label="Address line 1"
              autoComplete="address-line1"
              value={form.line1}
              onChange={set('line1')}
              error={fieldErrors.line1}
            />
            <Field
              id="line2"
              label="Address line 2 (optional)"
              autoComplete="address-line2"
              value={form.line2}
              onChange={set('line2')}
              error={fieldErrors.line2}
            />
            <div className="field-row">
              <Field
                id="city"
                label="City"
                autoComplete="address-level2"
                value={form.city}
                onChange={set('city')}
                error={fieldErrors.city}
              />
              <Field
                id="state"
                label="State or region (optional)"
                autoComplete="address-level1"
                value={form.state}
                onChange={set('state')}
                error={fieldErrors.state}
              />
            </div>
            <div className="field-row">
              <Field
                id="postalCode"
                label="Postal code"
                autoComplete="postal-code"
                value={form.postalCode}
                onChange={set('postalCode')}
                error={fieldErrors.postalCode}
              />
              <Field
                id="country"
                label="Country code"
                autoComplete="country"
                value={form.country}
                onChange={set('country')}
                error={fieldErrors.country}
                hint="2 letters, for example US, GB or IN."
              />
            </div>

            <p className="form-error" role="alert">
              {formError}
            </p>

            {hasUnavailable && (
              <p className="form-error">
                Something in your cart is no longer available. Open the cart and remove it first.
              </p>
            )}

            <button
              className="btn btn-primary btn-block"
              type="submit"
              disabled={submitting || hasUnavailable}
            >
              {submitting ? 'Taking you to Stripe…' : 'Continue to payment'}
            </button>
            <p className="stripe-note">
              You will pay on Stripe’s secure page. Nexora never sees your card details.
            </p>
          </form>
        </section>

        <aside className="form-card summary-card" aria-label="Order summary">
          <h2>Order summary</h2>
          <ul className="summary-list">
            {cart.items.map((item) => (
              <li key={item.productId}>
                <span>
                  {item.title} <small>× {item.quantity}</small>
                </span>
                <strong>{money.format(item.lineTotal)}</strong>
              </li>
            ))}
          </ul>
          <div className="subtotal-line">
            <span>Subtotal</span>
            <strong>{money.format(cart.subtotal)}</strong>
          </div>
          <p className="page-note">Prices are checked again when you continue.</p>
        </aside>
      </div>
    </PageShell>
  )
}
