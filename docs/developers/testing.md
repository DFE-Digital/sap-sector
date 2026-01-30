# Testing – Developer Guide

---

## Test pyramid

1. Unit tests
2. Integration tests
3. Playwright end-to-end tests

---

## Unit tests

- fast and deterministic
- no real DB or network
- Arrange / Act / Assert

---

## Playwright

- test user journeys
- prefer role/text selectors
- avoid sleeps and brittle selectors

For testing decisions, see:
- `/docs/adrs/011-executing-unit-tests.md`
- `/docs/adrs/012-tool-for-ui-testing.md`
