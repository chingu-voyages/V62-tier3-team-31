export type ProductVisual = 'phone' | 'laptop' | 'headphones' | 'gaming'

export type Product = {
  id: 'phone' | 'aerobook' | 'pulse' | 'volt'
  name: string
  category: 'Smartphones' | 'Laptops' | 'Audio'
  spec: string
  price: number
  rating: number
  reviews: number
  stock: 'In stock' | 'Limited stock'
  visual: ProductVisual
}

export const products: Product[] = [
  {
    id: 'phone',
    name: 'Nexora One X',
    category: 'Smartphones',
    spec: '256GB · 6.7" AMOLED · 5G',
    price: 18999,
    rating: 4.8,
    reviews: 320,
    stock: 'In stock',
    visual: 'phone',
  },
  {
    id: 'aerobook',
    name: 'AeroBook 14',
    category: 'Laptops',
    spec: '14" 2.8K · 16GB RAM · 512GB SSD',
    price: 16499,
    rating: 4.7,
    reviews: 184,
    stock: 'In stock',
    visual: 'laptop',
  },
  {
    id: 'pulse',
    name: 'Pulse ANC Pro',
    category: 'Audio',
    spec: 'Wireless · Noise cancelling · 50h',
    price: 4499,
    rating: 4.6,
    reviews: 272,
    stock: 'Limited stock',
    visual: 'headphones',
  },
  {
    id: 'volt',
    name: 'Volt G15',
    category: 'Laptops',
    spec: '15.6" QHD · RTX-class graphics · 16GB RAM',
    price: 22999,
    rating: 4.7,
    reviews: 118,
    stock: 'In stock',
    visual: 'gaming',
  },
]

export const productById = new Map(products.map((product) => [product.id, product]))

export const money = new Intl.NumberFormat('en-ZA', {
  style: 'currency',
  currency: 'ZAR',
  maximumFractionDigits: 0,
})
