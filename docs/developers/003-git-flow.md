# Git Workflow

This document explains **how developers use Git day-to-day**.

The rationale for this approach is documented in ADRs.

---

## Branching

- `main` is always deployable
- Feature branches use the format:
    - `feature/<short-description>`
    - `bugfix/<short-description>`
    - `chore/<short-description>`

---

## Commits

Follow Conventional Commits:
- `feat:`
- `fix:`
- `chore:`
- `docs:`
- `test:`

---

## Pull requests

Pull requests should:
- be small and focused
- include tests where behaviour changes
- update documentation if developer behaviour changes
- pass all CI checks

For branching strategy rationale, see:
- `/docs/adrs/002-branching-strategy.md`
