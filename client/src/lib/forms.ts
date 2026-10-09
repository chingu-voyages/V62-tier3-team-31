// Small helpers shared by the login, register and checkout forms.

export type FieldErrors = Record<string, string>

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function isValidEmail(value: string) {
  return emailPattern.test(value.trim())
}

// Same rules as the backend: 8 to 64 characters, with a letter, a number and a special character.
export function passwordProblem(password: string) {
  if (password.length < 8 || password.length > 64) return 'Password must be 8 to 64 characters.'
  if (!/[A-Za-z]/.test(password)) return 'Password needs at least one letter.'
  if (!/\d/.test(password)) return 'Password needs at least one number.'
  if (!/[^A-Za-z0-9]/.test(password)) return 'Password needs at least one special character.'
  return ''
}

function lowerFirst(value: string) {
  return value.charAt(0).toLowerCase() + value.slice(1)
}

// The backend lists every failed field: { shippingName: ["..."] }.
// This keeps the first message per field and renames the key to the form's own name.
export function toFieldErrors(errors: Record<string, string[]>, stripPrefix = ''): FieldErrors {
  const result: FieldErrors = {}

  for (const [key, messages] of Object.entries(errors)) {
    const withoutPrefix =
      stripPrefix && key.toLowerCase().startsWith(stripPrefix.toLowerCase())
        ? key.slice(stripPrefix.length)
        : key
    const message = messages[0]
    if (message) result[lowerFirst(withoutPrefix)] = message
  }

  return result
}

// Only allow a redirect to a page on this site.
export function safeNextPath(value: string | null, fallback = '/') {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) {
    return fallback
  }
  return value
}
