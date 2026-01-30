# Overview for Developers

This document gives developers a **working understanding** of the service.

It intentionally stays high-level and practical.
For architectural rationale, see `/docs/architecture` and `/docs/adrs`.

---

## What this service does

This service allows schools and local authorities to:
- search for schools
- compare similar schools
- view detailed school information

It is a public-facing ASP.NET Core MVC application.

---

## Technology summary

- ASP.NET Core MVC (.NET)
- PostgreSQL for data storage
- Lucene for search
- DfE Sign-in for authentication
- GOV.UK / DfE Frontend
- Playwright for end-to-end testing

---

## How the code is organised (at a glance)

- **Web**: controllers, views, UI concerns
- **Core**: domain logic and application services
- **Infrastructure**: database, search, external integrations

Controllers → Services → Repositories / Search

---

## Where to find deeper detail

- Architecture diagrams & decisions: `/docs/architecture`
- Architecture Decision Records: `/docs/adrs`
