# Git Workflow

This document explains **how developers use Git day-to-day**.

The rationale for this approach is documented in ADRs.

---

## Branching

- `main` is always deployable

- feature/{trello-id}-{trello-description}
  * e.g. feature/1001-let-users-login-with-azure-credentials
  
- bug/{trello-id}-{trello-description}
  * e.g. bug/1002-azure-login-not-working-for-scunthorpe-la
---

## Commits

Commits should be descriptive and inline with the changes.

---

## Pull requests

Pull requests should:
- be small and focused
- include tests where behaviour changes
- update documentation if developer behaviour changes
- pass all CI checks

For branching strategy rationale, see:
- `/docs/adrs/002-branching-strategy.md`
