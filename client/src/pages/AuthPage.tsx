import { useState, type FormEvent } from 'react'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import { Field } from '../components/Field'
import { PageShell } from '../components/PageShell'
import { useAuth } from '../hooks/useAuth'
import { ApiError } from '../lib/api'
import {
  isValidEmail,
  passwordProblem,
  safeNextPath,
  toFieldErrors,
  type FieldErrors,
} from '../lib/forms'

type AuthPageProps = {
  mode: 'login' | 'register'
}

export function AuthPage({ mode }: AuthPageProps) {
  const isRegister = mode === 'register'
  const { user, loading, login, register } = useAuth()
  const [params] = useSearchParams()
  const next = safeNextPath(params.get('next'))

  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  // Already logged in, or just logged in: go where the visitor was heading.
  if (!loading && user) {
    return <Navigate to={next} replace />
  }

  const validate = () => {
    const errors: FieldErrors = {}
    if (isRegister && !firstName.trim()) errors.firstName = 'Enter your first name.'
    if (isRegister && !lastName.trim()) errors.lastName = 'Enter your last name.'
    if (!isValidEmail(email)) errors.email = 'Enter a valid email address.'
    if (isRegister) {
      const problem = passwordProblem(password)
      if (problem) errors.password = problem
    } else if (!password) {
      errors.password = 'Enter your password.'
    }
    return errors
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError('')

    const problems = validate()
    setFieldErrors(problems)
    if (Object.keys(problems).length > 0) return

    setSubmitting(true)
    try {
      if (isRegister) {
        await register({
          firstName: firstName.trim(),
          lastName: lastName.trim(),
          email: email.trim(),
          password,
        })
      } else {
        await login(email.trim(), password)
      }
      // The user is now set, so the redirect above takes over.
    } catch (error) {
      setSubmitting(false)
      if (error instanceof ApiError) {
        const fields = toFieldErrors(error.errors)
        setFieldErrors(fields)
        if (Object.keys(fields).length === 0) setFormError(error.message)
      } else {
        setFormError('Something went wrong. Please try again.')
      }
    }
  }

  const otherPage = isRegister ? '/login' : '/register'
  const otherLink = params.get('next') ? `${otherPage}?next=${encodeURIComponent(next)}` : otherPage

  return (
    <PageShell>
      <section className="form-card auth-card">
        <p className="section-kicker">{isRegister ? 'Create account' : 'Welcome back'}</p>
        <h1>{isRegister ? 'Join Nexora' : 'Log in'}</h1>

        <form noValidate onSubmit={handleSubmit}>
          {isRegister && (
            <div className="field-row">
              <Field
                id="firstName"
                label="First name"
                autoComplete="given-name"
                value={firstName}
                onChange={setFirstName}
                error={fieldErrors.firstName}
              />
              <Field
                id="lastName"
                label="Last name"
                autoComplete="family-name"
                value={lastName}
                onChange={setLastName}
                error={fieldErrors.lastName}
              />
            </div>
          )}

          <Field
            id="email"
            label="Email address"
            type="email"
            autoComplete="email"
            value={email}
            onChange={setEmail}
            error={fieldErrors.email}
          />

          <Field
            id="password"
            label="Password"
            type="password"
            autoComplete={isRegister ? 'new-password' : 'current-password'}
            value={password}
            onChange={setPassword}
            error={fieldErrors.password}
            hint={isRegister ? '8 to 64 characters with a letter, a number and a special character.' : undefined}
          />

          <p className="form-error" role="alert">
            {formError}
          </p>

          <button className="btn btn-primary btn-block" type="submit" disabled={submitting}>
            {submitting ? 'Please wait…' : isRegister ? 'Create account' : 'Log in'}
          </button>
        </form>

        <p className="auth-switch">
          {isRegister ? 'Already have an account?' : 'New to Nexora?'}{' '}
          <Link to={otherLink}>{isRegister ? 'Log in' : 'Create an account'}</Link>
        </p>
      </section>
    </PageShell>
  )
}
