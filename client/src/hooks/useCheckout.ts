import { useState } from 'react'

export function useCheckout(cartCount: number) {
  const [checkoutMessage, setCheckoutMessage] = useState('')

  const resetCheckoutMessage = () => {
    setCheckoutMessage('')
  }

  const handleCheckout = () => {
    if (cartCount === 0) {
      setCheckoutMessage('Add at least one product before checkout.')
      return
    }

    const checkoutWindow = window as Window & { NEXORA_STRIPE_CHECKOUT_URL?: unknown }
    const hostedUrl = checkoutWindow.NEXORA_STRIPE_CHECKOUT_URL

    if (typeof hostedUrl === 'string' && /^https:\/\/checkout\.stripe\.com\//.test(hostedUrl)) {
      window.location.assign(hostedUrl)
      return
    }

    setCheckoutMessage(
      'Frontend demo only: connect your backend to create a Stripe Checkout Session, then redirect to session.url.',
    )
  }

  return {
    checkoutMessage,
    handleCheckout,
    resetCheckoutMessage,
  }
}
