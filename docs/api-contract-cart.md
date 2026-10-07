# API Contract: Cart

| Endpoint | URL | Access | Backlog |
| --- | --- | --- | --- |
| Get cart | `GET /api/v1/cart` | Public | B2 |
| Add item | `POST /api/v1/cart/items` | Public | B2 |
| Update quantity | `PATCH /api/v1/cart/items/{productId}` | Public | B2 |
| Remove item | `DELETE /api/v1/cart/items/{productId}` | Public | B2 |
| Clear cart | `DELETE /api/v1/cart` | Public | B2 |

## Shared

**Access**

All five are public, because guests can shop before signing up. Who owns the cart is worked out from the request, not from a login requirement:

- **Logged in** (valid `access_token` cookie) → the cart with that `user_id`
- **Guest** (no access token) → the cart with that `session_id`

Every endpoint uses the same rule, so it's described once here and not repeated below.

**Guest carts and the `cart_session` cookie**

| Cookie | Expires | Path |
| --- | --- | --- |
| `cart_session` | 30 days | `/` |

HttpOnly, Secure, SameSite matching the auth cookies.

- The value is a random opaque string. It is not guessable and it is not a user id.
- If a guest hits any cart endpoint without the cookie, the backend creates one and sets it on the response. A `GET /cart` from a brand new visitor returns an empty cart and a fresh cookie, not a 404.
- Logged-in users ignore this cookie entirely. It stays in the browser but is never used while an access token is present.

**Cart merging**

On successful register and on successful login, before returning the response:

1. If there's no `cart_session` cookie, or no guest cart for it, do nothing.
2. If the user has no cart, take the guest cart over by setting its `user_id` and clearing its `session_id`.
3. If the user already has a cart, move the guest items in: for a product in both, **add the quantities** and cap at available stock; for a product only in the guest cart, move the row across. Then delete the guest cart.
4. Delete the `cart_session` cookie.

This happens inside the login/register request, so the frontend does nothing. Its next `GET /cart` just shows the merged result.

**Cart object**

```json
{
  "items": [
    {
      "productId": "9c1f4a2e-5b7d-4e83-a1c6-2f8d3e7b9a04",
      "title": "Wireless Headphones",
      "imageUrl": "https://cdn.example.com/products/headphones.jpg",
      "unitPrice": 89.99,
      "quantity": 2,
      "lineTotal": 179.98,
      "stockQuantity": 4,
      "available": true
    }
  ],
  "itemCount": 2,
  "subtotal": 179.98
}
```

- The cart is keyed on `productId`, not on the cart-item id. The frontend never needs to know cart-item ids, so they aren't returned and the URLs use `productId`.
- `unitPrice` is read live from the product, never stored on the cart. Prices in a cart follow the current catalogue price. Prices are only frozen at checkout, into `order_items`.
- `lineTotal` is `unitPrice × quantity`, and `subtotal` is the sum of the line totals. Both are calculated by the backend, so the frontend never adds money up itself.
- `itemCount` is the total of all quantities, not the number of rows. Two of one product is `2`. This is the number on the cart badge in the header.
- `stockQuantity` is capped at 10, same rule as the products contract.
- `available` is `false` when the product has gone inactive or is out of stock since it was added. The row still comes back so the UI can show it greyed out with "no longer available".
- Items are ordered oldest added first, so the cart doesn't reshuffle when a quantity changes.
- No shipping, tax or discount fields. Those belong to checkout, not the cart.

**Errors**

Same global handler and shape as the other contracts.

```json
{ "title": "Product not found", "status": 404 }
```

400 validation errors also carry `errors`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "quantity": ["Quantity must be between 1 and 99."]
  }
}
```

**Quantity rules**, used by add and update:

- integer, 1 to 99
- never more than the product's real stock. Over stock returns 409, not 400, because the input was valid and the world changed.

---

## Get cart
`GET /api/v1/cart` · Public · B2

No body.

**200:** the cart object. An empty cart is `{ "items": [], "itemCount": 0, "subtotal": 0 }`, still a 200.

No errors except 500.

Sets `cart_session` if the caller is a guest without one.

---

## Add item
`POST /api/v1/cart/items` · Public · B2

Body: `{ productId, quantity }`, both required.

- `productId`: a UUID
- `quantity`: quantity rules

**200:** the whole cart object, not just the added row. The frontend replaces its cart state with the response and never patches it locally.

| Code | When |
| --- | --- |
| 400 | missing fields, bad UUID, or quantity outside 1 to 99 |
| 404 | no such product, or the product is inactive. Message: "Product not found" |
| 409 | not enough stock. Message: "Only 4 left in stock" |

Adding a product that's already in the cart **adds to** the existing quantity, it doesn't replace it. The combined quantity still has to pass the quantity rules, so adding 3 to an existing 98 returns 400.

---

## Update quantity
`PATCH /api/v1/cart/items/{productId}` · Public · B2

Body: `{ quantity }`, required. This **sets** the quantity, it doesn't add to it.

**200:** the whole cart object.

| Code | When |
| --- | --- |
| 400 | bad UUID, or quantity outside 1 to 99 |
| 404 | that product isn't in the cart. Message: "Item not in cart" |
| 409 | not enough stock |

Quantity 0 is a 400, not a delete. Removing is its own endpoint, so the frontend has to be explicit.

---

## Remove item
`DELETE /api/v1/cart/items/{productId}` · Public · B2

No body.

**200:** the whole cart object.

| Code | When |
| --- | --- |
| 400 | bad UUID |
| 404 | that product isn't in the cart |

---

## Clear cart
`DELETE /api/v1/cart` · Public · B2

No body.

**200:** the empty cart object.

Returns 200 whether or not the cart had anything in it. Used by the "clear cart" button and after a successful checkout.
