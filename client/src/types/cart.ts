import type { Product } from '../data/products'

export type CartQuantities = Partial<Record<Product['id'], number>>
