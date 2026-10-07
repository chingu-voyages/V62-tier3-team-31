# API Contract: Checkout & Stripe

| Endpoint | URL | Access | Backlog |
| --- | --- | --- | --- |
| Create checkout session | `POST /api/v1/checkout/session` | Needs login | B3 |
| Stripe webhook | `POST /api/v1/webhooks/stripe` | Stripe only | B3 |

Orders are read through the orders contract, not this one. This contract only covers taking money.

## Shared

**The flow, end to end**

1. React posts the shipping address to `POST /checkout/session`.
2. Backend validates the cart, freezes prices, writes an order with status `pending`, creates a Stripe Checkout Session, and returns its URL.
3. React sends the browser to that URL. Stripe collects the card, not us.
4. Stripe redirects back to our success or cancel page.
5. **Separately**, Stripe calls our webhook. The webhook, not the redirect, is what marks the order paid.

The redirect and the webhook are independent. A user closing the tab after paying must still get their order, so nothing in step 5 depends on step 4 happening.

**Money never comes from the client**

The request body has no prices, no totals and no product list. The backend reads the cart, reads current catalogue prices, and works out the total itself. A client that could send a price could send `0.01`.

**Prices are frozen at this point**

`order_items` stores `product_title` and `unit_price` copied from the product at checkout time. Later catalogue price changes never alter a placed order. This is why `order_items` keeps its own title rather than joining to `products`.

**Order statuses**

`status` — the money: `pending`, `paid`, `failed`, `cancelled`, `refunded`, `partially_refunded`, `disputed`
`fulfillment_status` — the parcel: `unfulfilled`, `processing`, `shipped`, `delivered`, `cancelled`

They move independently. A paid order starts `unfulfilled`. Only `status` is touched by this contract; fulfilment is set by hand for the MVP.

**Errors**

Same global handler and shape as the other contracts. Stripe errors are never passed through raw — a card decline from Stripe's API becomes our own 409 with a plain message.

---

## Create checkout session
`POST /api/v1/checkout/session` · Needs login · B3

Guests cannot check out. A guest gets 401 and React sends them to login, where the cart merge runs and brings their items with them.

**Body**

```json
{
  "shippingName": "Rola Herculean",
  "shippingPhone": "+1 555 0100",
  "shippingLine1": "221B Baker Street",
  "shippingLine2": null,
  "shippingCity": "London",
  "shippingState": null,
  "shippingPostalCode": "NW1 6XE",
  "shippingCountry": "GB"
}
```

| Field | Required | Rules |
| --- | --- | --- |
| `shippingName` | yes | trim, 1 to 200 |
| `shippingPhone` | no | trim, max 30 |
| `shippingLine1` | yes | trim, 1 to 255 |
| `shippingLine2` | no | trim, max 255 |
| `shippingCity` | yes | trim, 1 to 100 |
| `shippingState` | no | trim, max 100 |
| `shippingPostalCode` | yes | trim, 1 to 20 |
| `shippingCountry` | yes | ISO 3166-1 alpha-2, uppercased |

We collect the address ourselves rather than letting Stripe do it, because the order row needs it whether or not payment ever completes.

**201**

```json
{
  "orderId": "1f4c8b2a-6d3e-4790-b8a5-0c2e9f7d1a36",
  "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_..."
}
```

React redirects to `checkoutUrl`. It does not render Stripe itself.

| Code | When |
| --- | --- |
| 400 | validation failed |
| 401 | not logged in |
| 409 | cart is empty. Message: "Your cart is empty" |
| 409 | a product is inactive or deleted. Message: "Wireless Headphones is no longer available" |
| 409 | not enough stock. Message: "Only 4 of Wireless Headphones left in stock" |

**What the backend does, in order**

1. Load the user's cart with its products. Empty → 409.
2. Re-check every line: active, and `quantity <= stock_quantity`. Any failure → 409, naming the product so the UI can say which.
3. Work out the total from current prices.
4. In one transaction, write the `orders` row (`status = pending`, `paid_at` null) and its `order_items` with frozen `product_title` and `unit_price`.
5. Create the Stripe Checkout Session with `client_reference_id` set to our `orderId`, and `metadata.order_id` as well.
6. Save `stripe_session_id` on the order.
7. Return 201.

**Stock is not reduced here.** It comes off when payment succeeds, in the webhook. Reserving stock for unpaid orders needs a timeout job to release it, which isn't worth it for the MVP. The trade-off is that two people can check out the last item and one gets refunded; the webhook handles that case explicitly.

**The cart is not cleared here** either. It's cleared when payment succeeds, so a user who abandons Stripe still has their cart.

**Success and cancel URLs** point at the frontend: `/checkout/success?orderId=...` and `/checkout/cancel?orderId=...`. The success page shows "we're confirming your payment" and polls `GET /orders/{id}` until `status` leaves `pending`, because the webhook may land a second or two after the redirect.

---

## Stripe webhook
`POST /api/v1/webhooks/stripe` · Stripe only · B3

Called by Stripe, never by our frontend. Not behind auth, and it must not be behind CORS.

**Signature verification comes first.** Read the raw request body, verify the `Stripe-Signature` header against the webhook signing secret, and reject anything that fails with 400 before parsing. Without this, anyone who knows the URL can mark orders paid.

The endpoint must read the **raw body**. If model binding parses the JSON first, the signature won't match.

**Idempotency**

Stripe guarantees at-least-once delivery, not exactly-once. The same event will arrive twice, so the handler must be safe to run twice.

1. Insert the event id into `stripe_events`. The table's primary key is that id, so a duplicate insert fails.
2. On duplicate → return 200 immediately and do nothing else. Already handled.
3. Otherwise process it, then commit.

The insert and the processing go in **one transaction**, so a crash mid-way rolls the marker back too and Stripe's retry can do the work properly.

**Write first, acknowledge after**

The 200 goes back only after the database transaction has committed. No background tasks, no fire-and-forget. Returning 200 early tells Stripe the work is done and it will never retry, so a later failure loses the order silently.

**Events handled**

| Event | What we do |
| --- | --- |
| `checkout.session.completed` | the important one, below |
| `checkout.session.expired` | `status` → `cancelled` |
| `payment_intent.payment_failed` | `status` → `failed`, store the reason in `failure_reason` |
| `charge.refunded` | set `refunded_amount` to the charge's `amount_refunded` (Stripe sends the running total, so a repeat can't double count); `status` → `refunded` if it now equals the total, otherwise `partially_refunded` |
| `charge.dispute.created` | `status` → `disputed` |

Any other event: record it in `stripe_events` and return 200. Unknown events are not errors.

**On `checkout.session.completed`**, in one transaction:

1. Find the order by `client_reference_id`. Missing → log it and return 200, because retrying won't help.
2. If `status` is already `paid`, stop. Duplicate.
3. Set `status = paid`, `paid_at = now()`, and save `stripe_payment_intent_id`.
4. Reduce `stock_quantity` for each line.
5. Empty the user's cart.
6. Commit, then return 200.

Only act when the session's `payment_status` is `paid`. A `failed` order can still become `paid` if the customer retries inside the same Stripe session.

**If stock went negative** at step 4 — the case where two people bought the last item — still mark the order paid, because the money was taken. Set `stock_quantity` to 0 rather than a negative, and flag the order for a manual refund by writing a note in `failure_reason`. Never silently drop the payment.

**Responses to Stripe**

| Code | When |
| --- | --- |
| 200 | processed, duplicate, or an event we ignore |
| 400 | signature verification failed |
| 500 | our database failed. Stripe retries, which is what we want |

Never return 400 for a real event we simply failed to handle. 400 tells Stripe to stop retrying.

**Testing**

Use the Stripe CLI to forward events locally:

```
stripe listen --forward-to https://localhost:7178/api/v1/webhooks/stripe
```

Its signing secret differs from the dashboard's, so it goes in the Development config only.

---

## Open questions

- **Currency.** The products contract says USD. Stripe needs it set explicitly and it must match what we display.
- **Refunds.** No admin endpoint for issuing them in the MVP, so they're done from the Stripe dashboard and the webhook picks them up. That works, but nobody can refund from our app.
