import { money, productById, type Product } from '../data/products'

export type CartQuantities = Partial<Record<Product['id'], number>>

type CartDrawerProps = {
  isOpen: boolean
  quantities: CartQuantities
  checkoutMessage: string
  onClose: () => void
  onChangeQuantity: (id: Product['id'], delta: number) => void
  onRemove: (id: Product['id']) => void
  onCheckout: () => void
}

export function CartDrawer({
  isOpen,
  quantities,
  checkoutMessage,
  onClose,
  onChangeQuantity,
  onRemove,
  onCheckout,
}: CartDrawerProps) {
  const entries = Object.entries(quantities)
    .map(([id, quantity]) => {
      const product = productById.get(id as Product['id'])
      return product && quantity ? { product, quantity } : null
    })
    .filter((entry): entry is { product: Product; quantity: number } => entry !== null)
  const subtotal = entries.reduce(
    (total, { product, quantity }) => total + product.price * quantity,
    0,
  )

  return (
    <>
      <div
        aria-hidden="true"
        className={`drawer-backdrop${isOpen ? ' visible' : ''}`}
        hidden={!isOpen}
        onClick={onClose}
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
          <button className="icon-button" type="button" aria-label="Close cart" onClick={onClose}>
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M6 6l12 12M18 6 6 18" />
            </svg>
          </button>
        </div>

        <div className="cart-items">
          {entries.map(({ product, quantity }) => (
            <div className="cart-item" data-visual={product.visual} key={product.id}>
              <div className="cart-thumb">
                <div className="cart-thumb-shape" />
              </div>
              <div className="cart-item-info">
                <h3>{product.name}</h3>
                <p>{product.spec}</p>
                <div className="qty" aria-label="Quantity controls">
                  <button
                    type="button"
                    aria-label={`Decrease ${product.name} quantity`}
                    onClick={() => onChangeQuantity(product.id, -1)}
                  >
                    −
                  </button>
                  <span>{quantity}</span>
                  <button
                    type="button"
                    aria-label={`Increase ${product.name} quantity`}
                    onClick={() => onChangeQuantity(product.id, 1)}
                  >
                    +
                  </button>
                </div>
              </div>
              <div className="cart-item-side">
                <strong>{money.format(product.price * quantity)}</strong>
                <button className="remove-item" type="button" onClick={() => onRemove(product.id)}>
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className={`cart-empty${entries.length === 0 ? ' visible' : ''}`}>
          <div className="cart-empty-icon">N</div>
          <h3>Your cart is empty</h3>
          <p>Add something from the featured collection to get started.</p>
        </div>

        <div className="cart-summary">
          <div className="subtotal-line">
            <span>Subtotal</span>
            <strong>{money.format(subtotal)}</strong>
          </div>
          <p>Delivery is calculated at checkout.</p>

          <button className="btn btn-primary btn-block" type="button" onClick={onCheckout}>
            Secure checkout
          </button>
          <p className="stripe-note">
            Redirects to Stripe-hosted Checkout. Nexora does not collect card details on this page.
          </p>
          <p className="checkout-message" aria-live="polite">
            {checkoutMessage}
          </p>

          <button className="btn btn-secondary btn-block" type="button" onClick={onClose}>
            Continue shopping
          </button>
        </div>
      </aside>
    </>
  )
}
