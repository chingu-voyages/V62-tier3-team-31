import type { CategorySlug } from './products'

export type StorefrontCategory = {
  slug: CategorySlug
  navLabel: string
  order: string
  title: string
  description: string
  cta: string
  cardClassName: string
  artClassName: string
}

export const storefrontCategories: StorefrontCategory[] = [
  {
    slug: 'smartphones',
    navLabel: 'Phones',
    order: '01',
    title: 'Smartphones',
    description: 'Flagship cameras, fast displays and reliable all-day power.',
    cta: 'Shop smartphones',
    cardClassName: 'category-smartphones',
    artClassName: 'category-phone-art',
  },
  {
    slug: 'laptops',
    navLabel: 'Laptops',
    order: '02',
    title: 'Laptops',
    description: 'Portable productivity with premium displays and serious performance.',
    cta: 'Shop laptops',
    cardClassName: 'category-laptops',
    artClassName: 'category-laptop-art',
  },
  {
    slug: 'audio',
    navLabel: 'Audio',
    order: '03',
    title: 'Audio',
    description: 'Immersive sound, clean design and wireless freedom.',
    cta: 'Shop audio',
    cardClassName: 'category-audio',
    artClassName: 'category-headphone-art',
  },
]

export const promoCategorySlug: CategorySlug = 'laptops'
