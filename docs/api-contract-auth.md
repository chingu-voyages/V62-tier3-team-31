# API Contract: Auth

| Endpoint | URL | Access | Backlog |
| --- | --- | --- | --- |
| Register | `POST /api/v1/auth/register` | Public | A3 |
| Login | `POST /api/v1/auth/login` | Public | A4 |
| Refresh | `POST /api/v1/auth/refresh` | Public | A4 |
| Logout | `POST /api/v1/auth/logout` | Public | A4 |
| Me | `GET /api/v1/auth/me` | Needs login | A4 |
| Forgot password | `POST /api/v1/auth/forgot-password` | Public | A5 (stretch) |
| Reset password | `POST /api/v1/auth/reset-password` | Public | A5 (stretch) |

## Shared

**Access**
- Public: no token needed
- Needs login: valid `access_token` cookie, otherwise 401

**User object** (`{ user }` in responses below)

```json
{
  "user": {
    "id": "3f2b8c1e-7a4d-4e9b-9c2a-1d5e6f7a8b9c",
    "email": "rola@gmail.com",
    "firstName": "Rola",
    "lastName": "Herculean",
    "createdAt": "2026-09-15T14:30:00Z"
  }
}
```

Never includes the password hash. Tokens are never in the body, only in cookies.

**Auth cookies** ("both auth cookies" below)

| Cookie | Expires | Path |
| --- | --- | --- |
| `access_token` | 15 min | `/` |
| `refresh_token` | 7 days | `/api/v1/auth/refresh` |

Both are HttpOnly, Secure, SameSite=None.
(If we use a Vercel rewrite instead of calling Render directly, change to SameSite=Lax.)

**Errors**

All errors (except 400 validation) come from one global error handler on the backend, so every endpoint returns the same shape.

```json
{ "title": "Invalid email or password", "status": 401 }
```

400 validation errors also have `errors`, with every failed field at once:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "email": ["Email format is invalid."],
    "password": ["Password must be at least 8 characters."]
  }
}
```

Any endpoint can return 500. Prod: only "Something went wrong". Dev: also `detail` with the full error.

**Password rules** (register and reset password only)
- 8 to 64 characters
- at least 1 letter, 1 number, 1 special character (anything not a letter or number)
- never trimmed

---

## Register
`POST /api/v1/auth/register` · Public · A3

Body: `{ firstName, lastName, email, password }`, all required.
- firstName, lastName: trim, then 1 to 100 chars. Allow hyphens, apostrophes, accents.
- email: trim + lowercase first, valid format, max 255
- password: password rules

**201:** sets both auth cookies, returns `{ user }`. User is logged in straight away.

| Code | When |
| --- | --- |
| 400 | validation failed |
| 409 | "Email is already registered" |

Backend checks the rules again even though the frontend does too.

---

## Login
`POST /api/v1/auth/login` · Public · A4

Body: `{ email, password }`, both required.
- email: trim + lowercase before lookup
- password: don't trim, only check it's not empty. Don't use the password rules here, or old users get locked out if the rules change.

**200:** sets both auth cookies, returns `{ user }`

| Code | When |
| --- | --- |
| 400 | missing field or bad email format |
| 401 | wrong email or password. Same message for both: "Invalid email or password" |

---

## Refresh
`POST /api/v1/auth/refresh` · Public · A4

No body. Uses the `refresh_token` cookie.

**204:** sets a new `access_token` cookie. The refresh token isn't replaced, so users log in again 7 days after login.

| Code | When |
| --- | --- |
| 401 | refresh token missing, expired, invalid, or user deleted. Message: "Please log in again" |

**Frontend:**
- On any 401, call refresh once, then retry the request once.
- If several requests fail together, make only one refresh call and let the others wait for it.
- Don't run this for login, register, or refresh itself.
- If refresh fails, the user is logged out. Only protected pages redirect to login.

---

## Logout
`POST /api/v1/auth/logout` · Public · A4

No body.

**204:** clears both auth cookies. Always 204, even if not logged in.

No errors except 500.
- Delete cookies with the same options (path) they were set with, or they won't clear.
- It's public so users with an expired access token can still log out.
- Known limit: a copied refresh token keeps working until it expires (no token storage).

---

## Me
`GET /api/v1/auth/me` · Needs login · A4

No body.

**200:** returns `{ user }`

| Code | When |
| --- | --- |
| 401 | not logged in, token expired or invalid, or user deleted. Message: "Not logged in" |

- Token holds only the user ID. Always load the user from the DB.
- Frontend calls this on app load. A 401 here is normal for new visitors, so show the logged-out view, no error message.

---

## Forgot password
`POST /api/v1/auth/forgot-password` · Public · A5 (stretch)

Body: `{ email }`, required. Trim + lowercase, valid format.

**204:** always, for any valid email, whether it has an account or not. Frontend shows "If that email is registered, we've sent a reset link."

| Code | When |
| --- | --- |
| 400 | missing or bad email format |

If the account exists:
1. Create a random token.
2. Save only its hash in `password_reset_tokens`, expiring in 30 min.
3. Email a link to the frontend: `/reset-password?token=<token>`

---

## Reset password
`POST /api/v1/auth/reset-password` · Public · A5 (stretch)

Body: `{ token, password }`, both required.
- token: from the page URL, don't change it
- password: password rules

**204:** password changed. Not logged in, so the frontend sends them to login.

| Code | When |
| --- | --- |
| 400 | validation failed (has `errors`) |
| 400 | token wrong, used, or expired. Message: "This reset link is invalid or has expired" (no `errors`) |

In one transaction:
1. Save the new password hash.
2. Set `used_at` on this token.
3. Mark the user's other unused reset tokens as used.

Known limit: doesn't log out other devices.
