# Security for Developers

This document covers **secure coding practices**.

---

## Rules

- never commit secrets
- never log sensitive data
- treat all input as untrusted
- keep authorisation checks server side

---

## CSP

- no inline scripts without nonce
- no inline event handlers
- review new external domains


---

## Cookies and sessions

- HttpOnly, Secure and SameSite on every cookie the service sets
- never put personal data in a cookie value
- do not extend session lifetimes without agreeing it with the team

---

## Data access

- use parameterised Dapper queries, never string concatenation
- never build SQL from user supplied input

For security strategy, see

- `/docs/security/001-overview.md`
- `/docs/testing/007-security-tests.md`
- `/docs/adrs/008-secrets-management.md`
