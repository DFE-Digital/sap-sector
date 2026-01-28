# Security for Developers

This document covers **secure coding practices**.

---

## Rules

- never commit secrets
- never log sensitive data
- treat all input as untrusted

---

## CSP

- no inline scripts without nonce
- no inline event handlers
- review new external domains

For security strategy, see:
- `/docs/adrs/008-secrets-management.md`
