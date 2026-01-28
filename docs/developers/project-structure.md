# Project Structure

This document explains **where new code should live**.

---

## Feature-based organisation

Prefer feature folders in Web:

SAPSec.Web/Features/Search/

Avoid dumping everything into:
- Controllers/
- Models/
- Helpers/

---

## Responsibility boundaries

- Web: HTTP and UI concerns
- Core: business rules and use-cases
- Infrastructure: data and integrations

If you are unsure where something goes, stop and ask.
