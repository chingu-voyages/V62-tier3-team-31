export type ProductVisual = 'phone' | 'laptop' | 'headphones' | 'gaming'

export type CategorySlug = 'smartphones' | 'laptops' | 'audio'

export type ProductCategory = {
  id: string
  name: string
  slug: CategorySlug
}

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

export const catalogCategories: ProductCategory[] = [
  {
    id: '2b8d4a1c-5b85-4a27-934f-4f6c76f2e501',
    name: 'Smartphones',
    slug: 'smartphones',
  },
  {
    id: '8f0a7df6-22c2-4900-8e47-b3d2d6be8bc6',
    name: 'Laptops',
    slug: 'laptops',
  },
  {
    id: 'd4c2d79d-b3c3-4b26-85e8-dad8f89dbcd1',
    name: 'Audio',
    slug: 'audio',
  },
]

const smartphonesCategory = catalogCategories[0]
const laptopsCategory = catalogCategories[1]
const audioCategory = catalogCategories[2]

export const products: Product[] = [
  {
    id: '1f623603-229d-40ac-a52f-5c040efb174a',
    title: 'Nexora One X',
    description: '256GB · 6.7" AMOLED · 5G',
    price: 18999,
    stockQuantity: 8,
    imageUrl: null,
    category: smartphonesCategory,
    visual: 'phone',
  },
  {
    id: 'bc224020-0bf0-4eb4-88b2-1fe2b0408a86',
    title: 'AeroBook 14',
    description: '14" 2.8K · 16GB RAM · 512GB SSD',
    price: 16499,
    stockQuantity: 6,
    imageUrl: null,
    category: laptopsCategory,
    visual: 'laptop',
  },
  {
    id: 'cf578350-5640-4d0e-b676-1fc32b6c0c83',
    title: 'Pulse ANC Pro',
    description: 'Wireless · Noise cancelling · 50h',
    price: 4499,
    stockQuantity: 3,
    imageUrl: null,
    category: audioCategory,
    visual: 'headphones',
  },
  {
    id: '1303f95d-a3b1-4697-8721-72c624d7e9e9',
    title: 'Volt G15',
    description: '15.6" QHD · RTX-class graphics · 16GB RAM',
    price: 22999,
    stockQuantity: 5,
    imageUrl: null,
    category: laptopsCategory,
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

export const money = new Intl.NumberFormat('en-ZA', {
  style: 'currency',
  currency: 'ZAR',
  maximumFractionDigits: 0,
})
