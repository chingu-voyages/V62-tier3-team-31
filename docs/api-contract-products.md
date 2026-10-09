# API Contract: Products

| Endpoint | URL | Access | Backlog |
| --- | --- | --- | --- |
| List products | `GET /api/v1/products` | Public | B1 |
| Product detail | `GET /api/v1/products/{id}` | Public | B1 |
| List categories | `GET /api/v1/categories` | Public | B1 |

## Shared

**Access**

All three are public. No cookie needed, and none of them ever look at the logged-in user.

**Product object**

```json
{
  "id": "9c1f4a2e-5b7d-4e83-a1c6-2f8d3e7b9a04",
  "title": "Wireless Headphones",
  "description": "Over-ear Bluetooth headphones with 30-hour battery.",
  "price": 89.99,
  "stockQuantity": 4,
  "imageUrl": "https://cdn.example.com/products/air-max-90.jpg",
  "category": {
    "id": "3a7e1c9b-2d4f-4a86-b5e1-7c9d0f2a4b63",
    "name": "Electronics",
    "slug": "electronics"
  }
}
```

- `price` is a decimal with 2 places, in USD. Never a string, and never cents — `89.99`, not `8999`.
- `stockQuantity` is capped at 10 in the response. Real stock of 500 is sent as 10, so we don't publish exact inventory. The frontend only needs the number when it's low ("Only 4 left").
- `description` and `imageUrl` can be `null`.
- `isActive` is never returned. It's an admin flag, and inactive products simply don't appear.

**Category object**

```json
{
  "id": "3a7e1c9b-2d4f-4a86-b5e1-7c9d0f2a4b63",
  "name": "Electronics",
  "slug": "electronics"
}
```

`createdAt` and `updatedAt` aren't returned anywhere in this contract. Nothing in the UI uses them.

**Paged response**

Every list endpoint that pages uses this shape:

```json
{
  "items": [],
  "page": 1,
  "limit": 20,
  "total": 137,
  "totalPages": 7
}
```

`total` is the count of everything matching the filters, not the count on this page.

**Errors**

Same global error handler and same shape as the auth contract.

```json
{ "title": "Product not found", "status": 404 }
```

400 validation errors also carry `errors` with every failed field:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "limit": ["Limit must be between 1 and 50."]
  }
}
```

Any endpoint can return 500. Prod: only "Something went wrong". Dev: also `detail` with the full error.

---

## List products
`GET /api/v1/products` · Public · B1

Query parameters, all optional:

| Param | Type | Default | Rules |
| --- | --- | --- | --- |
| `page` | int | 1 | 1 or more |
| `limit` | int | 20 | 1 to 50 |
| `category` | string | — | a category `slug`, not an id |
| `search` | string | — | trim, max 100 chars, matches title only |
| `sort` | string | `newest` | one of `newest`, `price_asc`, `price_desc`, `title_asc` |

**200:** paged response with `items` as an array of product objects.

| Code | When |
| --- | --- |
| 400 | any parameter fails its rules |

- Only products with `isActive = true` are ever returned.
- An unknown `category` slug returns 200 with an empty `items` array, not 404. The category filter is a filter, not a lookup.
- `search` is case-insensitive and matches anywhere in the title, not just the start.
- `search` and `category` combine: both are applied.
- A `page` beyond the last one returns 200 with empty `items`. `total` and `totalPages` still reflect the real counts, so the frontend can send the user back to page 1.
- Out-of-stock products are still returned, with `stockQuantity: 0`. The frontend shows them greyed out rather than hiding them.

**Examples**

```
GET /api/v1/products
GET /api/v1/products?page=2&limit=12
GET /api/v1/products?category=electronics&sort=price_asc
GET /api/v1/products?search=headphones
```

---

## Product detail
`GET /api/v1/products/{id}` · Public · B1

No body. `{id}` is the product's UUID.

**200:** a single product object, not wrapped.

| Code | When |
| --- | --- |
| 400 | `{id}` isn't a valid UUID |
| 404 | no product with that id, or the product has `isActive = false`. Message: "Product not found" |

An inactive product returns 404, not 403. We don't tell anyone a hidden product exists.

---

## List categories
`GET /api/v1/categories` · Public · B1

No body, no query parameters, no paging. There will only ever be a handful of categories.

**200:**

```json
{ "categories": [] }
```

An array of category objects, sorted by `name` ascending.

No errors except 500.

The frontend uses this for the nav and the filter dropdown, so the `slug` here is exactly what gets passed to `GET /products?category=`.
