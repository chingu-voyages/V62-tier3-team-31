import { useMemo, useState, type ChangeEvent } from 'react'
import { catalogCategories, type CategorySlug } from '../data/products'
import type { Product } from '../types/api'

export function useProductFilters(productList: Product[]) {
  const [activeCategory, setActiveCategory] = useState<CategorySlug | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const displayedProducts = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase()

    return productList.filter((product) => {
      const categoryMatches = !activeCategory || product.category.slug === activeCategory
      const searchMatches =
        !normalizedSearch ||
        [product.title, product.category.name, product.category.slug, product.description ?? '']
          .join(' ')
          .toLowerCase()
          .includes(normalizedSearch)

      return categoryMatches && searchMatches
    })
  }, [activeCategory, productList, searchTerm])

  const activeCategoryLabel = activeCategory
    ? catalogCategories.find((category) => category.slug === activeCategory)?.name
    : null

  const filterLabels = [
    activeCategoryLabel,
    searchTerm.trim() ? `“${searchTerm.trim()}”` : null,
  ].filter((label): label is string => Boolean(label))

  const applyCategoryFilter = (category: CategorySlug) => {
    setActiveCategory(category)
    setSearchTerm('')
  }

  const clearFilters = () => {
    setActiveCategory(null)
    setSearchTerm('')
  }

  const handleSearch = (event: ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value)
    setActiveCategory(null)
  }

  return {
    activeCategory,
    searchTerm,
    displayedProducts,
    filterLabels,
    applyCategoryFilter,
    clearFilters,
    handleSearch,
  }
}
