// Products now come from the backend (GET /api/v1/products).
// This file keeps what the UI still needs locally: the category list used for the nav,
// the artwork choice for the demo products, and small formatting helpers.

export type ProductVisual = 'phone' | 'laptop' | 'headphones' | 'gaming' | 'generic'

// Same ids and slugs as the seeded categories in the database.
export const catalogCategories = [
  {
    id: '2b8d4a1c-5b85-4a27-934f-4f6c76f2e501',
    name: 'Electronics',
    slug: 'electronics',
  },
  {
    id: '8f0a7df6-22c2-4900-8e47-b3d2d6be8bc6',
    name: 'Books',
    slug: 'books',
  },
  {
    id: 'd4c2d79d-b3c3-4b26-85e8-dad8f89dbcd1',
    name: 'Kitchen',
    slug: 'kitchen',
  },
  {
    id: 'a0e6ccf6-555b-48c8-b4e6-26826a464e26',
    name: 'Stationery',
    slug: 'stationery',
  },
  {
    id: 'ec0d92a8-8ec8-4d21-8d2e-29c2f9c38430',
    name: 'Toys',
    slug: 'toys',
  },
] as const

export type CategorySlug = (typeof catalogCategories)[number]['slug']

// The four electronics have drawn artwork. Everything else gets a plain tile.
const visualsById: Record<string, ProductVisual> = {
  '1f623603-229d-40ac-a52f-5c040efb174a': 'phone',
  'bc224020-0bf0-4eb4-88b2-1fe2b0408a86': 'laptop',
  'cf578350-5640-4d0e-b676-1fc32b6c0c83': 'headphones',
  '1303f95d-a3b1-4697-8721-72c624d7e9e9': 'gaming',
}

export function visualFor(productId: string): ProductVisual {
  return visualsById[productId] ?? 'generic'
}

// The hero button adds this product.
export const heroProductId = '1f623603-229d-40ac-a52f-5c040efb174a'

// The API never sends a stock number above 10, so 10 means "10 or more".
export function getStockLabel(stockQuantity: number) {
  if (stockQuantity <= 0) return 'Out of stock'
  if (stockQuantity <= 3) return `${stockQuantity} left`
  if (stockQuantity >= 10) return 'In stock'
  return `${stockQuantity} in stock`
}

export function hasLimitedStock(stockQuantity: number) {
  return stockQuantity > 0 && stockQuantity <= 3
}

// Whole dollars stay whole ($999), anything else shows cents ($34.99).
export const money = {
  format(value: number) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: Number.isInteger(value) ? 0 : 2,
      maximumFractionDigits: 2,
    }).format(value)
  },
}
