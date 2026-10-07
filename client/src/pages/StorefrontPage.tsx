import { useEffect, useState } from 'react'
import { CartDrawer } from '../components/CartDrawer'
import { DeviceArt } from '../components/DeviceArt'
import {
  getStockLabel,
  hasLimitedStock,
  heroProductId,
  money,
  products,
  type CategorySlug,
  type Product,
} from '../data/products'
import { promoCategorySlug, storefrontCategories } from '../data/storefrontCategories'
import { useCart } from '../hooks/useCart'
import { useCheckout } from '../hooks/useCheckout'
import { useNewsletterForm } from '../hooks/useNewsletterForm'
import { useProductFilters } from '../hooks/useProductFilters'

type ProductCardProps = {
  product: Product
  onAddToCart: (id: Product['id']) => void
}

function ProductCard({ product, onAddToCart }: ProductCardProps) {
  const stockLabel = getStockLabel(product.stockQuantity)

  return (
    <article className="product-card">
      <div className="product-media">
        <DeviceArt visual={product.visual} />
      </div>
      <div className="product-body">
        <h3 className="product-name">{product.title}</h3>
        <p className="product-spec">{product.description}</p>
        <div className="product-price-row">
          <span className="product-price">{money.format(product.price)}</span>
          <span className={`stock${hasLimitedStock(product.stockQuantity) ? ' limited' : ''}`}>
            {stockLabel}
          </span>
        </div>
        <button className="btn btn-primary" type="button" onClick={() => onAddToCart(product.id)}>
          Add to cart
        </button>
      </div>
    </article>
  )
}

function HeroPhoneArtwork() {
  return (
    <div
      className="hero-visual"
      role="img"
      aria-label="Nexora One X smartphone illustration"
    >
      <div className="phone-shadow" />
      <div className="phone phone-back">
        <div className="camera-island">
          <span />
          <span />
          <span />
          <i />
        </div>
        <div className="phone-logo">N</div>
      </div>
      <div className="phone phone-front">
        <div className="phone-speaker" />
        <div className="phone-screen">
          <div className="screen-glow screen-glow-a" />
          <div className="screen-glow screen-glow-b" />
          <div className="screen-time">09:41</div>
          <div className="screen-caption">
            NEXORA
            <br />
            <span>ONE X</span>
          </div>
        </div>
      </div>
      <div className="hero-floating-badge">
        <span>5G</span>
        <small>Flagship performance</small>
      </div>
    </div>
  )
}

export function StorefrontPage() {
  const [isCartOpen, setIsCartOpen] = useState(false)
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false)
  const {
    cartQuantities,
    cartCount,
    addToCart,
    changeQuantity,
    removeFromCart,
  } = useCart()
  const {
    checkoutMessage,
    handleCheckout,
    resetCheckoutMessage,
  } = useCheckout(cartCount)
  const {
    newsletterMessage,
    newsletterSucceeded,
    handleNewsletterSubmit,
  } = useNewsletterForm()
  const {
    searchTerm,
    displayedProducts,
    filterLabels,
    applyCategoryFilter,
    clearFilters,
    handleSearch,
  } = useProductFilters(products)

  useEffect(() => {
    if (!isCartOpen) {
      return
    }

    const savedOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'

    return () => {
      document.body.style.overflow = savedOverflow
    }
  }, [isCartOpen])

  useEffect(() => {
    const closeCartOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsCartOpen(false)
      }
    }

    document.addEventListener('keydown', closeCartOnEscape)
    return () => document.removeEventListener('keydown', closeCartOnEscape)
  }, [])

  const scrollToFeatured = () => {
    document.getElementById('featured')?.scrollIntoView({ behavior: 'smooth' })
  }

  const selectCategory = (category: CategorySlug) => {
    applyCategoryFilter(category)
    scrollToFeatured()
  }

  const handleAddToCart = (id: Product['id']) => {
    addToCart(id)
    resetCheckoutMessage()
    setIsCartOpen(true)
  }

  const handleChangeQuantity = (id: Product['id'], delta: number) => {
    changeQuantity(id, delta)
    resetCheckoutMessage()
  }

  const handleRemoveFromCart = (id: Product['id']) => {
    removeFromCart(id)
    resetCheckoutMessage()
  }

  return (
    <>
      <div className="announcement">Free delivery on orders over $50</div>

      <header className="site-header">
        <div className="header-inner container">
          <a className="brand" href="#top" aria-label="Nexora home">
            <span className="brand-mark" aria-hidden="true" />
            <span>Nexora</span>
          </a>

          <nav className="desktop-nav" aria-label="Primary navigation">
            <a href="#featured">Shop</a>
            {storefrontCategories.map((category) => (
              <a href="#featured" key={category.slug} onClick={() => selectCategory(category.slug)}>
                {category.navLabel}
              </a>
            ))}
            <a href="#promo">Deals</a>
          </nav>

          <div className="header-actions">
            <label className="search" aria-label="Search products">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="m21 21-4.35-4.35m2.35-5.65a8 8 0 1 1-16 0 8 8 0 0 1 16 0Z" />
              </svg>
              <input
                id="searchInput"
                type="search"
                placeholder="Search"
                autoComplete="off"
                value={searchTerm}
                onChange={handleSearch}
              />
            </label>

            <button className="icon-button" type="button" aria-label="Account">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M20 21a8 8 0 0 0-16 0m12-13a4 4 0 1 1-8 0 4 4 0 0 0 0-2Z" />
              </svg>
            </button>

            <button
              className="icon-button cart-button"
              type="button"
              aria-label="Open cart"
              onClick={() => setIsCartOpen(true)}
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M3 3h2l2 12h10l2-8H6m3 12a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm8 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2Z" />
              </svg>
              <span className="cart-count">{cartCount}</span>
            </button>

            <button
              className="menu-button"
              type="button"
              aria-label="Open menu"
              aria-expanded={isMobileNavOpen}
              onClick={() => setIsMobileNavOpen((isOpen) => !isOpen)}
            >
              <span />
              <span />
              <span />
            </button>
          </div>
        </div>

        <nav className={`mobile-nav${isMobileNavOpen ? ' open' : ''}`} aria-label="Mobile navigation">
          <a href="#featured" onClick={() => setIsMobileNavOpen(false)}>Shop</a>
          {storefrontCategories.map((category) => (
            <a
              href="#featured"
              key={category.slug}
              onClick={() => {
                selectCategory(category.slug)
                setIsMobileNavOpen(false)
              }}
            >
              {category.navLabel}
            </a>
          ))}
          <a href="#promo" onClick={() => setIsMobileNavOpen(false)}>Deals</a>
        </nav>
      </header>

      <main id="top">
        <section className="hero">
          <div className="hero-orb hero-orb-a" />
          <div className="hero-orb hero-orb-b" />

          <div className="container hero-grid">
            <div className="hero-copy">
              <div className="eyebrow">NEXORA ONE X</div>
              <h1>
                Meet the future
                <br />
                of mobile.
              </h1>
              <p className="hero-lead">
                A flagship smartphone engineered around a vivid AMOLED display, intelligent cameras and
                all-day performance.
              </p>

              <div className="hero-buy">
                <div>
                  <span className="hero-price-label">From</span>
                  <strong className="hero-price">$999</strong>
                </div>
                <button className="btn btn-primary" type="button" onClick={() => handleAddToCart(heroProductId)}>
                  Shop now
                </button>
              </div>

              <div className="hero-features">
                <div>
                  <span className="feature-icon">◎</span>
                  <strong>200MP</strong>
                  <small>AI Camera</small>
                </div>
                <div>
                  <span className="feature-icon">✦</span>
                  <strong>Next-gen</strong>
                  <small>Octa-core</small>
                </div>
                <div>
                  <span className="feature-icon">↯</span>
                  <strong>All-day</strong>
                  <small>Battery</small>
                </div>
              </div>
            </div>

            <HeroPhoneArtwork />
          </div>
        </section>

        <section id="categories" className="section categories-section">
          <div className="container">
            <div className="section-heading">
              <div>
                <p className="section-kicker">Shop by category</p>
                <h2>Everyday essentials, chosen for you.</h2>
              </div>
              <a className="text-link" href="#featured">
                View all products <span>→</span>
              </a>
            </div>

            <div className="category-grid">
              {storefrontCategories.map((category) => (
                <button
                  className={`category-card ${category.cardClassName}`}
                  key={category.slug}
                  type="button"
                  onClick={() => selectCategory(category.slug)}
                >
                  <div className="category-copy">
                    <span>{category.order}</span>
                    <h3>{category.title}</h3>
                    <p>{category.description}</p>
                    <strong>{category.cta} →</strong>
                  </div>
                  <div className="category-art" aria-hidden="true">
                    <span>{category.title.slice(0, 1)}</span>
                  </div>
                </button>
              ))}
            </div>
          </div>
        </section>

        <section id="featured" className="section featured-section">
          <div className="container">
            <div className="section-heading">
              <div>
                <p className="section-kicker">Featured collection</p>
                <h2>Built to perform.</h2>
              </div>
              <div className="product-filter-status" aria-live="polite">
                {filterLabels.length ? `Showing: ${filterLabels.join(' · ')}` : `${products.length} featured products`}
              </div>
            </div>

            <div className="product-grid" hidden={displayedProducts.length === 0}>
              {displayedProducts.map((product) => (
                <ProductCard key={product.id} product={product} onAddToCart={handleAddToCart} />
              ))}
            </div>

            <div className="empty-products" hidden={displayedProducts.length !== 0}>
              <h3>No products found</h3>
              <p>Try another search term or clear the current filter.</p>
              <button className="btn btn-secondary" type="button" onClick={clearFilters}>
                Clear filter
              </button>
            </div>
          </div>
        </section>

        <section id="promo" className="section promo-section">
          <div className="container">
            <div className="promo-card">
              <div className="promo-content">
                <p className="section-kicker light">Student offer</p>
                <h2>
                  Big ideas go further
                  <br />
                  with Nexora.
                </h2>
                <p>
                  Save on selected electronics built for lectures, creative work and everything after class.
                </p>
                <a
                  className="btn btn-light"
                  href="#featured"
                  onClick={(event) => {
                    event.preventDefault()
                    selectCategory(promoCategorySlug)
                  }}
                >
                  Explore the deal
                </a>
              </div>

              <div className="promo-art" aria-hidden="true">
                <div className="promo-laptop">
                  <div className="promo-laptop-screen">
                    <div className="promo-ui">
                      <span />
                      <span />
                      <span />
                    </div>
                  </div>
                  <div className="promo-laptop-base" />
                </div>
              </div>
            </div>
          </div>
        </section>

        <section className="newsletter-section">
          <div className="container newsletter-grid">
            <div>
              <p className="section-kicker">Stay in the loop</p>
              <h2>Get the latest from Nexora.</h2>
              <p>New arrivals, launch offers and practical buying guides — no clutter.</p>
            </div>

            <form className="newsletter-form" noValidate onSubmit={handleNewsletterSubmit}>
              <label htmlFor="newsletterEmail">Email address</label>
              <div className="newsletter-control">
                <input
                  id="newsletterEmail"
                  name="newsletterEmail"
                  type="email"
                  placeholder="you@example.com"
                  required
                />
                <button className="btn btn-primary" type="submit">
                  Subscribe
                </button>
              </div>
              <p className={`form-message${newsletterSucceeded ? ' success' : ''}`} aria-live="polite">
                {newsletterMessage}
              </p>
            </form>
          </div>
        </section>
      </main>

      <footer className="site-footer">
        <div className="container footer-grid">
          <div className="footer-brand">
            <a className="brand brand-footer" href="#top">
              <span className="brand-mark" aria-hidden="true" />
              <span>Nexora</span>
            </a>
            <p>Practical goods with clear pricing, helpful support and secure checkout.</p>
          </div>

          <div>
            <h3>Shop</h3>
            {storefrontCategories.map((category) => (
              <a href="#featured" key={category.slug} onClick={() => selectCategory(category.slug)}>
                {category.title}
              </a>
            ))}
            <a href="#promo">Deals</a>
          </div>

          <div>
            <h3>Support</h3>
            <a href="#delivery">Delivery</a>
            <a href="#returns">Returns</a>
            <a href="#warranty">Warranty</a>
            <a href="mailto:support@nexora.example">Contact</a>
          </div>

          <div>
            <h3>Confidence</h3>
            <p>Secure payment flow</p>
            <p>Clear returns process</p>
            <p>Responsive support</p>
          </div>
        </div>

        <div className="container footer-bottom">
          <span>© 2026 Nexora. Concept storefront.</span>
          <span>No real brand logos are used.</span>
        </div>
      </footer>

      <CartDrawer
        isOpen={isCartOpen}
        quantities={cartQuantities}
        checkoutMessage={checkoutMessage}
        onClose={() => setIsCartOpen(false)}
        onChangeQuantity={handleChangeQuantity}
        onRemove={handleRemoveFromCart}
        onCheckout={handleCheckout}
      />
    </>
  )
}
