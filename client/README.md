# Nexora React storefront

This is a faithful React + TypeScript port of the supplied
`Nexora-Storefront-Vanilla` reference. It keeps the reference storefront's
single-page layout, CSS-drawn product visuals, responsive breakpoints, and
demo interactions.

## Run locally

```bash
npm install
npm run dev
```

## Checks

```bash
npm run lint
npm run build
```

## Included

- Responsive header, mobile navigation, hero, category cards, featured products,
  promotion, newsletter, footer, and cart drawer
- Search and category filtering, in-memory cart quantities and ZAR subtotal
- Newsletter validation and Stripe-hosted Checkout handoff demo
- Four mock products and CSS-only visual artwork

The cart and checkout are frontend-only. To try a real Stripe-hosted Checkout
redirect, set `window.NEXORA_STRIPE_CHECKOUT_URL` to an existing Stripe Checkout
URL before clicking **Secure checkout**.
