// Leaves our site for another address (Stripe's payment page).
// Kept in its own file so tests can replace it.
export function redirectTo(url: string) {
  window.location.assign(url)
}
