import { useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from 'react'
import { CartDrawer, type CartQuantities } from '../components/CartDrawer'
import { DeviceArt } from '../components/DeviceArt'
import { money, products, type Product } from '../data/products'

type Category = Product['category']

type ProductCardProps = {
  product: Product
  onAddToCart: (id: Product['id']) => void
}

function ProductCard({ product, onAddToCart }: ProductCardProps) {
  const roundedRating = Math.round(product.rating)
  const ratingStars = `${'★'.repeat(roundedRating)}${'☆'.repeat(5 - roundedRating)}`

  return (
    <article className="product-card">
      <div className="product-media">
        <DeviceArt visual={product.visual} />
      </div>
      <div className="product-body">
        <h3 className="product-name">{product.name}</h3>
        <p className="product-spec">{product.spec}</p>
        <div className="rating-row" aria-label={`${product.rating} out of 5 stars`}>
          <span aria-hidden="true">{ratingStars}</span>
          <small>
            {product.rating} ({product.reviews})
          </small>
        </div>
        <div className="product-price-row">
          <span className="product-price">{money.format(product.price)}</span>
          <span className={`stock${product.stock.includes('Limited') ? ' limited' : ''}`}>
            {product.stock}
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
  const [activeCategory, setActiveCategory] = useState<Category | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [cartQuantities, setCartQuantities] = useState<CartQuantities>({})
  const [isCartOpen, setIsCartOpen] = useState(false)
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false)
  const [newsletterMessage, setNewsletterMessage] = useState('')
  const [newsletterSucceeded, setNewsletterSucceeded] = useState(false)
  const [checkoutMessage, setCheckoutMessage] = useState('')

  const displayedProducts = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase()

    return products.filter((product) => {
      const categoryMatches = !activeCategory || product.category === activeCategory
      const searchMatches =
        !normalizedSearch ||
        [product.name, product.category, product.spec]
          .join(' ')
          .toLowerCase()
          .includes(normalizedSearch)

      return categoryMatches && searchMatches
    })
  }, [activeCategory, searchTerm])

  const cartCount = Object.values(cartQuantities).reduce(
    (total, quantity) => total + (quantity ?? 0),
    0,
  )

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

  const applyCategoryFilter = (category: Category) => {
    setActiveCategory(category)
    setSearchTerm('')
    scrollToFeatured()
  }

  const clearFilters = () => {
    setActiveCategory(null)
    setSearchTerm('')
  }

  const handleSearch = (event: ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value)
    setActiveCategory(null)
  }

  const addToCart = (id: Product['id']) => {
    setCartQuantities((current) => ({
      ...current,
      [id]: (current[id] ?? 0) + 1,
    }))
    setCheckoutMessage('')
    setIsCartOpen(true)
  }

  const changeQuantity = (id: Product['id'], delta: number) => {
    setCartQuantities((current) => {
      const quantity = (current[id] ?? 0) + delta

      if (quantity <= 0) {
        const next = { ...current }
        delete next[id]
        return next
      }

      return { ...current, [id]: quantity }
    })
    setCheckoutMessage('')
  }

  const removeFromCart = (id: Product['id']) => {
    setCartQuantities((current) => {
      const next = { ...current }
      delete next[id]
      return next
    })
    setCheckoutMessage('')
  }

  const handleNewsletterSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const form = event.currentTarget
    const emailControl = form.elements.namedItem('newsletterEmail')
    const emailInput = emailControl instanceof HTMLInputElement ? emailControl : null

    if (!emailInput || !emailInput.validity.valid) {
      setNewsletterSucceeded(false)
      setNewsletterMessage('Enter a valid email address.')
      emailInput?.focus()
      return
    }

    setNewsletterSucceeded(true)
    setNewsletterMessage('Thanks — you’re on the Nexora list.')
    form.reset()
  }

  const handleCheckout = () => {
    if (cartCount === 0) {
      setCheckoutMessage('Add at least one product before checkout.')
      return
    }

    const checkoutWindow = window as Window & { NEXORA_STRIPE_CHECKOUT_URL?: unknown }
    const hostedUrl = checkoutWindow.NEXORA_STRIPE_CHECKOUT_URL

    if (typeof hostedUrl === 'string' && /^https:\/\/checkout\.stripe\.com\//.test(hostedUrl)) {
      window.location.assign(hostedUrl)
      return
    }

    setCheckoutMessage(
      'Frontend demo only: connect your backend to create a Stripe Checkout Session, then redirect to session.url.',
    )
  }

  const filterLabels = [
    activeCategory,
    searchTerm.trim() ? `“${searchTerm.trim()}”` : null,
  ].filter((label): label is string => Boolean(label))

  return (
    <>
      <div className="announcement">Free delivery on orders over R750</div>

      <header className="site-header">
        <div className="header-inner container">
          <a className="brand" href="#top" aria-label="Nexora home">
            <span className="brand-mark" aria-hidden="true" />
            <span>Nexora</span>
          </a>

          <nav className="desktop-nav" aria-label="Primary navigation">
            <a href="#featured">Shop</a>
            <a href="#categories" data-category-link="Smartphones">Phones</a>
            <a href="#categories" data-category-link="Laptops">Laptops</a>
            <a href="#categories" data-category-link="Audio">Audio</a>
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
          <a href="#categories" onClick={() => setIsMobileNavOpen(false)}>Phones</a>
          <a href="#categories" onClick={() => setIsMobileNavOpen(false)}>Laptops</a>
          <a href="#categories" onClick={() => setIsMobileNavOpen(false)}>Audio</a>
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
                  <strong className="hero-price">R18,999</strong>
                </div>
                <button className="btn btn-primary" type="button" onClick={() => addToCart('phone')}>
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
                <h2>Technology for every day.</h2>
              </div>
              <a className="text-link" href="#featured">
                View all products <span>→</span>
              </a>
            </div>

            <div className="category-grid">
              <button
                className="category-card category-smartphones"
                type="button"
                onClick={() => applyCategoryFilter('Smartphones')}
              >
                <div className="category-copy">
                  <span>01</span>
                  <h3>Smartphones</h3>
                  <p>Flagship cameras, fast displays and reliable all-day power.</p>
                  <strong>Shop smartphones →</strong>
                </div>
                <div className="category-art category-phone-art" aria-hidden="true">
                  <div className="mini-phone mini-phone-a" />
                  <div className="mini-phone mini-phone-b" />
                </div>
              </button>

              <button
                className="category-card category-laptops"
                type="button"
                onClick={() => applyCategoryFilter('Laptops')}
              >
                <div className="category-copy">
                  <span>02</span>
                  <h3>Laptops</h3>
                  <p>Portable productivity with premium displays and serious performance.</p>
                  <strong>Shop laptops →</strong>
                </div>
                <div className="category-art category-laptop-art" aria-hidden="true">
                  <div className="mini-laptop-screen" />
                  <div className="mini-laptop-base" />
                </div>
              </button>

              <button
                className="category-card category-audio"
                type="button"
                onClick={() => applyCategoryFilter('Audio')}
              >
                <div className="category-copy">
                  <span>03</span>
                  <h3>Audio</h3>
                  <p>Immersive sound, clean design and wireless freedom.</p>
                  <strong>Shop audio →</strong>
                </div>
                <div className="category-art category-headphone-art" aria-hidden="true">
                  <div className="headphone-band" />
                  <div className="headphone-cup cup-left" />
                  <div className="headphone-cup cup-right" />
                </div>
              </button>
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
                {filterLabels.length ? `Showing: ${filterLabels.join(' · ')}` : '4 featured products'}
              </div>
            </div>

            <div className="product-grid" hidden={displayedProducts.length === 0}>
              {displayedProducts.map((product) => (
                <ProductCard key={product.id} product={product} onAddToCart={addToCart} />
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
                  Save on selected laptops built for lectures, creative work and everything after class.
                </p>
                <a
                  className="btn btn-light"
                  href="#featured"
                  onClick={(event) => {
                    event.preventDefault()
                    applyCategoryFilter('Laptops')
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
            <p>Premium consumer electronics with clear pricing, practical support and secure checkout.</p>
          </div>

          <div>
            <h3>Shop</h3>
            <a href="#categories">Smartphones</a>
            <a href="#categories">Laptops</a>
            <a href="#categories">Audio</a>
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
        onChangeQuantity={changeQuantity}
        onRemove={removeFromCart}
        onCheckout={handleCheckout}
      />
    </>
  )
}
