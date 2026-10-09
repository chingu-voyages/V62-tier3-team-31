import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../hooks/useCart'
import { AccountMenu } from './AccountMenu'

// Header and footer for every page except the storefront, which has its own big layout.
export function PageShell({ children }: { children: ReactNode }) {
  const { cart, openCart } = useCart()

  return (
    <>
      <header className="site-header">
        <div className="header-inner container">
          <Link className="brand" to="/" aria-label="Nexora home">
            <span className="brand-mark" aria-hidden="true" />
            <span>Nexora</span>
          </Link>

          <nav className="desktop-nav" aria-label="Primary navigation">
            <Link to="/">Shop</Link>
            <Link to="/orders">My orders</Link>
          </nav>

          <div className="header-actions">
            <AccountMenu />

            <button
              className="icon-button cart-button"
              type="button"
              aria-label="Open cart"
              onClick={openCart}
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M3 3h2l2 12h10l2-8H6m3 12a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm8 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2Z" />
              </svg>
              <span className="cart-count">{cart.itemCount}</span>
            </button>
          </div>
        </div>
      </header>

      <main className="page-main container">{children}</main>

      <footer className="page-footer">
        <div className="container">
          <span>© 2026 Nexora. Concept storefront.</span>
        </div>
      </footer>
    </>
  )
}
