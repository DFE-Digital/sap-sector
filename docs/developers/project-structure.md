# Project Structure

This document describes **how the project is currently structured** and **where new code should live**.

---

## Current structure

The Web project **does not use feature folders**.

Code is organised by technical concern and should continue to follow the existing layout:

- `Controllers/` – MVC controllers and HTTP endpoints
- `ViewModels/` – View-specific models
- `Views/` – Razor views
- `Services/` – Web-layer services
- `Helpers/` – Small, reusable web utilities

Do **not** introduce feature folders or new structural patterns unless agreed by the team.

---

## Where to put new code

- UI / HTTP logic → **SAPSec.Web**
- Business logic or rules → **SAPSec.Core**
- Database access or integrations → **SAPSec.Infrastructure**

If similar code already exists, **follow the existing pattern**.

---

## Rules of thumb

- Keep controllers thin
- Do not put business logic in controllers
- Infrastructure should not depend on Web
- Prefer consistency over “better” structure

---

If you are unsure where something belongs, **stop and ask before adding it**.
