import { catalogCategories, type CategorySlug } from './products'

export type StorefrontCategory = {
  slug: CategorySlug
  navLabel: string
  order: string
  title: string
  description: string
  cta: string
  cardClassName: string
}

const categoryDescriptions: Record<CategorySlug, string> = {
  electronics: 'Devices and accessories for work, play and everyday life.',
  books: 'Stories, ideas and practical guides for every kind of reader.',
  kitchen: 'Useful essentials for preparing, serving and sharing meals.',
  stationery: 'Pens, paper and desk tools to keep ideas moving.',
  toys: 'Playful picks made for curious minds and big imaginations.',
}

export const storefrontCategories: StorefrontCategory[] = catalogCategories.map((category, index) => ({
  slug: category.slug,
  navLabel: category.name,
  order: String(index + 1).padStart(2, '0'),
  title: category.name,
  description: categoryDescriptions[category.slug],
  cta: `Shop ${category.name.toLowerCase()}`,
  cardClassName: `category-${category.slug}`,
}))

export const promoCategorySlug: CategorySlug = 'electronics'
