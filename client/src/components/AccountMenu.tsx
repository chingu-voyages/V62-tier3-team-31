import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const accountIcon = (
  <svg viewBox="0 0 24 24" aria-hidden="true">
    <path d="M20 21a8 8 0 0 0-16 0m12-13a4 4 0 1 1-8 0 4 4 0 0 0 0-2Z" />
  </svg>
)

// The person icon in the header. Logged out it goes to the login page.
// Logged in it opens a small menu with their orders and log out.
export function AccountMenu() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const wrapperRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return

    const closeOnOutsideClick = (event: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) setOpen(false)
    }
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }

    document.addEventListener('mousedown', closeOnOutsideClick)
    document.addEventListener('keydown', closeOnEscape)
    return () => {
      document.removeEventListener('mousedown', closeOnOutsideClick)
      document.removeEventListener('keydown', closeOnEscape)
    }
  }, [open])

  if (!user) {
    return (
      <Link className="icon-button" to="/login" aria-label="Log in">
        {accountIcon}
      </Link>
    )
  }

  const handleLogout = async () => {
    setOpen(false)
    await logout()
    navigate('/')
  }

  return (
    <div className="account-menu" ref={wrapperRef}>
      <button
        className="icon-button"
        type="button"
        aria-label="Account"
        aria-expanded={open}
        onClick={() => setOpen((isOpen) => !isOpen)}
      >
        {accountIcon}
      </button>

      {open && (
        <div className="account-popup" role="menu">
          <p className="account-name">Hi, {user.firstName}</p>
          <Link role="menuitem" to="/orders" onClick={() => setOpen(false)}>
            My orders
          </Link>
          <button role="menuitem" type="button" onClick={handleLogout}>
            Log out
          </button>
        </div>
      )}
    </div>
  )
}
