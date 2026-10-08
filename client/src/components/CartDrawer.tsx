import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { money, visualFor } from '../data/products'
import { useAuth } from '../hooks/useAuth'
import { useCart } from '../hooks/useCart'

export function CartDrawer() {
  const { cart, message, isOpen, closeCart, setQuantity, removeItem } = useCart()
  const { user } = useAuth()
  const navigate = useNavigate()

  const isEmpty = cart.items.length === 0
  const hasUnavailable = cart.items.some((item) => !item.available)

  // Lock the page behind the drawer while it is open.
  useEffect(() => {
    if (!isOpen) return

    const savedOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'

    return () => {
      document.body.style.overflow = savedOverflow
    }
  }, [isOpen])

  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') closeCart()
    }

    document.addEventListener('keydown', closeOnEscape)
    return () => document.removeEventListener('keydown', closeOnEscape)
  }, [closeCart])

  const handleCheckout = () => {
    closeCart()
    // Guests log in first. Logging in merges their cart, then they come back here.
    navigate(user ? '/checkout' : '/login?next=%2Fcheckout')
  }

  return (
    <>
      <div
        aria-hidden="true"
        className={`drawer-backdrop${isOpen ? ' visible' : ''}`}
        hidden={!isOpen}
        onClick={closeCart}
      />

      <aside
        aria-hidden={!isOpen}
        aria-label="Shopping cart"
        className={`cart-drawer${isOpen ? ' open' : ''}`}
      >
        <div className="cart-header">
          <div>
            <p className="section-kicker">Your cart</p>
            <h2>Shopping bag</h2>
          </div>
          <button className="icon-button" type="button" aria-label="Close cart" onClick={closeCart}>
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M6 6l12 12M18 6 6 18" />
            </svg>
          </button>
        </div>

        <div className="cart-items">
          {cart.items.map((item) => (
            <div
              className={`cart-item${item.available ? '' : ' unavailable'}`}
              data-visual={visualFor(item.productId)}
              key={item.productId}
            >
              <div className="cart-thumb">
                {item.imageUrl ? (
                  <img className="cart-thumb-photo" src={item.imageUrl} alt="" loading="lazy" />
                ) : (
                  <div className="cart-thumb-shape" />
                )}
              </div>
              <div className="cart-item-info">
                <h3>{item.title}</h3>
                {item.available ? (
                  <div className="qty" aria-label="Quantity controls">
                    <button
                      type="button"
                      aria-label={`Decrease ${item.title} quantity`}
                      onClick={() =>
                        item.quantity <= 1
                          ? removeItem(item.productId)
                          : setQuantity(item.productId, item.quantity - 1)
                      }
                    >
                      −
                    </button>
                    <span>{item.quantity}</span>
                    <button
                      type="button"
                      aria-label={`Increase ${item.title} quantity`}
                      disabled={item.quantity >= 99}
                      onClick={() => setQuantity(item.productId, item.quantity + 1)}
                    >
                      +
                    </button>
                  </div>
                ) : (
                  <p className="cart-item-note">No longer available. Remove it to check out.</p>
                )}
              </div>
              <div className="cart-item-side">
                <strong>{item.available ? money.format(item.lineTotal) : '—'}</strong>
                <button className="remove-item" type="button" onClick={() => removeItem(item.productId)}>
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className={`cart-empty${isEmpty ? ' visible' : ''}`}>
          <div className="cart-empty-icon">N</div>
          <h3>Your cart is empty</h3>
          <p>Add something from the featured collection to get started.</p>
        </div>

        <div className="cart-summary">
          <div className="subtotal-line">
            <span>Subtotal</span>
            <strong>{money.format(cart.subtotal)}</strong>
          </div>
          <p>Delivery is calculated at checkout.</p>

          <button
            className="btn btn-primary btn-block"
            type="button"
            disabled={isEmpty || hasUnavailable}
            onClick={handleCheckout}
          >
            Secure checkout
          </button>
          <p className="stripe-note">
            Redirects to Stripe-hosted Checkout. Nexora does not collect card details on this page.
          </p>
          <p className="checkout-message" aria-live="polite">
            {message}
          </p>

          <button className="btn btn-secondary btn-block" type="button" onClick={closeCart}>
            Continue shopping
          </button>
        </div>
      </aside>
    </>
  )
}
