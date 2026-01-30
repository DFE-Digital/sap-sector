# Authentication (DfE Sign-in)

This document explains how authentication is used **in code**.

---

## Developer rules

- Use [Authorize] explicitly
- Prefer policy-based authorisation
- Treat claims as untrusted input
- Never log tokens or secrets

For rationale, see:
- `/docs/adrs/009-authentication-provider.md`
