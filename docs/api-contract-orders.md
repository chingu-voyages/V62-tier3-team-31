# API Contract: Orders

| Endpoint | URL | Access | Backlog |
| --- | --- | --- | --- |
| List my orders | `GET /api/v1/orders` | Needs login | B4 |
| Order detail | `GET /api/v1/orders/{id}` | Needs login | B4 |

## Shared

**Access**

Both need a valid `access_token` cookie, otherwise 401 "Not logged in", same as `GET /auth/me`.

A user only ever sees their own orders. The owner is the user ID in the token, never anything the client sends.

**Read only**

Nothing in this contract creates, changes or cancels an order. Orders are created by checkout, and their status is changed by the Stripe webhook. Refunds are issued from the Stripe dashboard.

**Money**

Same rules as the products contract: decimals with 2 places, in the store currency, never strings and never cents.

**Order object** (detail)

```json
{
  "id": "1f4c8b2a-6d3e-4790-b8a5-0c2e9f7d1a36",
  "status": "paid",
  "fulfillmentStatus": "shipped",
  "totalAmount": 179.98,
  "refundedAmount": 0,
  "shipping": {
    "name": "Rola Herculean",
    "phone": "+1 555 0100",
    "line1": "221B Baker Street",
    "line2": null,
    "city": "London",
    "state": null,
    "postalCode": "NW1 6XE",
    "country": "GB"
  },
  "items": [
    {
      "productId": "9c1f4a2e-5b7d-4e83-a1c6-2f8d3e7b9a04",
      "title": "Wireless Headphones",
      "imageUrl": "https://cdn.example.com/products/headphones.jpg",
      "unitPrice": 89.99,
      "quantity": 2,
      "lineTotal": 179.98
    }
  ],
  "createdAt": "2026-10-01T15:20:00Z",
  "paidAt": "2026-10-01T15:22:41Z"
}
```

- `status` is one of `pending`, `paid`, `failed`, `cancelled`, `refunded`, `partially_refunded`, `disputed`.
- `fulfillmentStatus` is one of `unfulfilled`, `processing`, `shipped`, `delivered`, `cancelled`.
- `title` and `unitPrice` are the **frozen** values copied at checkout, not the current catalogue values. A price change later never alters an old order.
- `imageUrl` is read live from the product, because order items don't store one. It can be `null`.
- `lineTotal` is `unitPrice × quantity`, worked out by the backend.
- `shipping` is nested here, although checkout takes the same fields flat with a `shipping` prefix. `phone`, `line2` and `state` can be `null`.
- `paidAt` is `null` until payment succeeds.
- Never returned: `userId`, `stripeSessionId`, `stripePaymentIntentId` and `failureReason`. These are internal, and `failureReason` can hold raw Stripe text.

**Order summary object** (list)

```json
{
  "id": "1f4c8b2a-6d3e-4790-b8a5-0c2e9f7d1a36",
  "status": "paid",
  "fulfillmentStatus": "shipped",
  "totalAmount": 179.98,
  "itemCount": 2,
  "createdAt": "2026-10-01T15:20:00Z",
  "paidAt": "2026-10-01T15:22:41Z"
}
```

`itemCount` is the total of all quantities, not the number of rows. No `items` or `shipping` here. The list stays small and the detail endpoint has the rest.

**Paged response**

Same shape as the products contract:

```json
{
  "items": [],
  "page": 1,
  "limit": 10,
  "total": 3,
  "totalPages": 1
}
```

**Errors**

Same global handler and shape as the other contracts.

```json
{ "title": "Order not found", "status": 404 }
```

400 validation errors also carry `errors` with every failed field.

---

## List my orders
`GET /api/v1/orders` · Needs login · B4

Query parameters, all optional:

| Param | Type | Default | Rules |
| --- | --- | --- | --- |
| `page` | int | 1 | 1 or more |
| `limit` | int | 10 | 1 to 50 |

**200:** paged response with `items` as an array of order summary objects.

| Code | When |
| --- | --- |
| 400 | a parameter fails its rules |
| 401 | not logged in |

- Only orders that have been **paid at some point** are listed, meaning `paidAt` is not null. That includes orders since marked `refunded`, `partially_refunded` or `disputed`.
- Orders still `pending`, and ones that were abandoned (`cancelled` after the Stripe session expired) or `failed`, are **not** listed. Every time someone opens checkout and walks away, an order row is created, and listing those would fill the history with noise.
- Newest first by `paidAt`. Ties are broken by `id`, so paging is stable and an order never repeats or disappears between pages.
- A user with no orders gets 200 with empty `items`, `total: 0` and `totalPages: 0`.
- A `page` beyond the last returns 200 with empty `items`, with real totals.

---

## Order detail
`GET /api/v1/orders/{id}` · Needs login · B4

No body. `{id}` is the order's UUID.

**200:** a single order object, not wrapped.

| Code | When |
| --- | --- |
| 400 | `{id}` isn't a valid UUID |
| 401 | not logged in |
| 404 | no such order, **or it belongs to another user**. Message: "Order not found" |

Someone else's order returns 404, not 403. We don't confirm that an order id exists.

Unlike the list, the detail endpoint returns the user's own orders **in any status**, including `pending`. The checkout success page depends on this. It polls this endpoint after Stripe redirects back, until `status` leaves `pending`, because the webhook can land a second or two after the redirect.
