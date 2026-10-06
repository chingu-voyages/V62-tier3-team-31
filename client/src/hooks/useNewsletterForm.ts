import { useState, type FormEvent } from 'react'

export function useNewsletterForm() {
  const [newsletterMessage, setNewsletterMessage] = useState('')
  const [newsletterSucceeded, setNewsletterSucceeded] = useState(false)

  const handleNewsletterSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const form = event.currentTarget
    const emailControl = form.elements.namedItem('newsletterEmail')
    const emailInput = emailControl instanceof HTMLInputElement ? emailControl : null

    if (!emailInput || !emailInput.validity.valid) {
      setNewsletterSucceeded(false)
      setNewsletterMessage('Enter a valid email address.')
      emailInput?.focus()
      return
    }

    setNewsletterSucceeded(true)
    setNewsletterMessage('Thanks — you’re on the Nexora list.')
    form.reset()
  }

  return {
    newsletterMessage,
    newsletterSucceeded,
    handleNewsletterSubmit,
  }
}
