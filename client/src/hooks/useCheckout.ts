import { useState } from 'react'
import { ApiError, api } from '../lib/api'
import { toFieldErrors, type FieldErrors } from '../lib/forms'
import { redirectTo } from '../lib/redirect'
import type { CheckoutRequest, CheckoutResponse } from '../types/api'

export type ShippingForm = {
  name: string
  phone: string
  line1: string
  line2: string
  city: string
  state: string
  postalCode: string
  country: string
}

const emptyToNull = (value: string) => {
  const trimmed = value.trim()
  return trimmed === '' ? null : trimmed
}

function validate(form: ShippingForm): FieldErrors {
  const errors: FieldErrors = {}
  if (!form.name.trim()) errors.name = 'Enter the name of the person receiving the order.'
  if (!form.line1.trim()) errors.line1 = 'Enter the street address.'
  if (!form.city.trim()) errors.city = 'Enter the city.'
  if (!form.postalCode.trim()) errors.postalCode = 'Enter the postal code.'
  if (!/^[A-Za-z]{2}$/.test(form.country.trim())) {
    errors.country = 'Use the 2-letter country code, for example US, GB or IN.'
  }
  return errors
}

// Sends the address to the backend, then hands the browser over to Stripe.
export function useCheckout() {
  const [submitting, setSubmitting] = useState(false)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState('')

  const submit = async (form: ShippingForm) => {
    setFormError('')

    const problems = validate(form)
    setFieldErrors(problems)
    if (Object.keys(problems).length > 0) return

    const body: CheckoutRequest = {
      shippingName: form.name.trim(),
      shippingPhone: emptyToNull(form.phone),
      shippingLine1: form.line1.trim(),
      shippingLine2: emptyToNull(form.line2),
      shippingCity: form.city.trim(),
      shippingState: emptyToNull(form.state),
      shippingPostalCode: form.postalCode.trim(),
      shippingCountry: form.country.trim().toUpperCase(),
    }

    setSubmitting(true)
    try {
      const result = await api<CheckoutResponse>('/checkout/session', { method: 'POST', body })

      // Only ever send the browser to Stripe's own site.
      if (!result.checkoutUrl.startsWith('https://checkout.stripe.com/')) {
        throw new Error('Unexpected payment address')
      }

      redirectTo(result.checkoutUrl)
      // Stay in the "submitting" state while the browser leaves the page.
    } catch (error) {
      setSubmitting(false)

      if (error instanceof ApiError) {
        const fields = toFieldErrors(error.errors, 'shipping')
        setFieldErrors(fields)
        // A 409 has no field errors. Its title already says what is wrong,
        // for example "Only 1 of Pulse ANC Pro left in stock".
        if (Object.keys(fields).length === 0) setFormError(error.message)
      } else {
        setFormError('We could not start the payment. Please try again.')
      }
    }
  }

  return { submitting, fieldErrors, formError, submit }
}
