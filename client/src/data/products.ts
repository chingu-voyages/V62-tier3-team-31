export type ProductVisual = 'phone' | 'laptop' | 'headphones' | 'gaming'

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

export type ProductCategory = (typeof catalogCategories)[number]
export type CategorySlug = ProductCategory['slug']

export type Product = {
  id: string
  title: string
  description: string
  price: number
  stockQuantity: number
  imageUrl: string | null
  category: ProductCategory
  visual: ProductVisual
}

const electronicsCategory = catalogCategories[0]

export const products: Product[] = [
  {
    id: '1f623603-229d-40ac-a52f-5c040efb174a',
    title: 'Nexora One X',
    description: '256GB · 6.7" AMOLED · 5G',
    price: 999,
    stockQuantity: 8,
    imageUrl: null,
    category: electronicsCategory,
    visual: 'phone',
  },
  {
    id: 'bc224020-0bf0-4eb4-88b2-1fe2b0408a86',
    title: 'AeroBook 14',
    description: '14" 2.8K · 16GB RAM · 512GB SSD',
    price: 899,
    stockQuantity: 6,
    imageUrl: null,
    category: electronicsCategory,
    visual: 'laptop',
  },
  {
    id: 'cf578350-5640-4d0e-b676-1fc32b6c0c83',
    title: 'Pulse ANC Pro',
    description: 'Wireless · Noise cancelling · 50h',
    price: 249,
    stockQuantity: 3,
    imageUrl: null,
    category: electronicsCategory,
    visual: 'headphones',
  },
  {
    id: '1303f95d-a3b1-4697-8721-72c624d7e9e9',
    title: 'Volt G15',
    description: '15.6" QHD · RTX-class graphics · 16GB RAM',
    price: 1299,
    stockQuantity: 5,
    imageUrl: null,
    category: electronicsCategory,
    visual: 'gaming',
  },
]

export const productById = new Map(products.map((product) => [product.id, product]))

export const heroProductId = products[0].id

export function getStockLabel(stockQuantity: number) {
  if (stockQuantity <= 0) {
    return 'Out of stock'
  }

  if (stockQuantity <= 3) {
    return `${stockQuantity} left`
  }

  return `${stockQuantity} in stock`
}

export function hasLimitedStock(stockQuantity: number) {
  return stockQuantity > 0 && stockQuantity <= 3
}

export const money = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})
