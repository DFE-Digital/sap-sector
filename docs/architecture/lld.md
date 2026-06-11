# Low Level Design (LLD) – SAP Sector

## 1. Purpose

This document provides the **Low Level Design (LLD)** for the SAP Sector service.

It describes:
- internal service components and boundaries
- data types and models used at each layer
- user types and how requests flow through the system
- how services, repositories, and search interact

This document exists to support:
- assurance and assessment
- developer onboarding
- clear understanding of internal interactions

For a high-level overview, see `/docs/architecture/hld.md`.  
For architectural rationale, see `/docs/adrs/`.

---

## 2. Architectural layering

The service uses a **layered architecture** with strict dependency rules.

### Layers

1. **Web (ASP.NET Core MVC)**
2. **Core (Domain + Application)**
3. **Infrastructure**
4. **Data stores**

### Dependency rules

- Web depends on Core
- Core defines interfaces and business logic
- Infrastructure implements Core interfaces
- Infrastructure depends on external systems (Postgres, Lucene)
- Dependencies always point **inwards**
- Controllers must never access repositories or Lucene directly

---

## 3. User types (implementation view)

### 3.1 End users

- **Unauthenticated users**
    - Access public routes (if enabled)
    - Cannot access protected features

- **Authenticated users (DfE Sign-in)**
    - Identified by DfE Sign-in
    - Access controlled via ASP.NET Core authorisation
    - Claims treated as untrusted input

### 3.2 Operational users

- **Service operators**
    - Monitor health endpoints
    - Investigate logs and telemetry
    - Perform operational tasks (e.g. reindex) if implemented

---

## 4. Web layer (SAPSec.Web)

### Responsibilities

- Handle HTTP requests and routing
- Enforce authentication and authorisation
- Validate input (model binding / ModelState)
- Map request data into application DTOs
- Call application services (use-cases)
- Map service results into ViewModels
- Render Razor views or return appropriate responses

### Key rules

- Controllers must be thin
- No database access
- No Lucene access
- No business rules
- No infrastructure types exposed to views

### Typical components

- Controllers (feature-based)
- ViewModels
- Razor Views
- Filters and middleware (cross-cutting)

---

## 5. Application layer (SAPSec.Core)

### Responsibilities

The application layer implements **use-cases** that represent user actions.

Examples:
- Search schools
- View school details
- Compare similar schools

Each use-case:
- accepts input DTOs
- validates business rules
- coordinates repositories and search services
- returns result DTOs or result objects

### Key rules

- No MVC dependencies
- No direct database or Lucene access
- Depends only on interfaces
- Testable without ASP.NET hosting

---

## 6. Infrastructure layer (SAPSec.Infrastructure)

### Responsibilities

- Implement repository interfaces (PostgreSQL)
- Implement search interfaces (Lucene)
- Handle indexing and querying logic
- Integrate with external systems if required
- Map persistence/search results into DTOs

### Key rules

- Owns all Postgres and Lucene details
- No MVC dependencies
- Does not expose ORM or Lucene types outside the layer

---

## 7. Data access (PostgreSQL)

### Repository design

Repositories:
- are async-only
- expose domain-oriented methods
- return DTOs or domain models (not ORM entities)
- enforce paging for list operations

Examples:
- Get school by URN
- Get multiple schools by URN list
- Load attributes required for comparison

### Data rules

- PostgreSQL is the authoritative data store
- All queries must be parameterised
- Avoid N+1 queries
- Use appropriate indexes (documented in ERD)

---

## 8. Search subsystem (Lucene)

### Responsibilities

- Provide fast full-text and filtered search
- Support paging and ordering
- Return candidate results for further processing

### Search rules

- Lucene is **not** authoritative
- Input must be sanitised and escaped
- Query length and result limits enforced
- Lucene-specific types never escape Infrastructure

### Indexing rules

- Index schema defined in one place
- Index derived from authoritative data
- Reindex required when schema or key data changes

---

## 9. Data models and boundaries

### ViewModels (Web)
- Used only for rendering views
- May contain formatted/display-friendly values
- Must not include persistence or search types

### Application DTOs (Core)
- Represent inputs and outputs of use-cases
- Independent of UI and infrastructure
- Used in services and tests

### Domain models (Core)
- Represent business concepts and rules
- No UI or persistence concerns

### Persistence/search models (Infrastructure)
- Map to SQL rows or Lucene documents
- Must not be exposed outside Infrastructure

---

## 10. Key interactions (low-level flows)

### 10.1 Search schools

1. Controller receives search input
2. Input validated at Web boundary
3. Search use-case invoked with DTO
4. Search service calls Lucene implementation
5. Lucene returns paged result DTO
6. Controller maps to ViewModel
7. View rendered

---

### 10.2 View school details

1. Controller receives school identifier
2. Use-case invoked
3. Repository queried (PostgreSQL)
4. Data mapped to DTO
5. Controller maps DTO to ViewModel
6. View rendered

---

### 10.3 Compare similar schools

1. Controller invokes compare use-case
2. Use-case determines similar school set:
    - via rules and/or Lucene and/or stored relationships
3. Repository loads authoritative details
4. DTO assembled for comparison
5. Controller maps to ViewModel
6. Comparison view rendered

---

## 11. Authentication and authorisation (DfE Sign-in)

### Implementation rules

- Protected routes explicitly use `[Authorize]`
- Policy-based authorisation used for complex rules
- Claims treated as untrusted input
- Tokens and headers are never logged
- Services do not read `HttpContext` directly

---

## 12. Error handling and logging

### Error handling

- Expected outcomes returned as result objects
- Unexpected exceptions handled globally
- User-facing errors are safe and non-technical

### Logging

- Structured logging only
- Avoid PII, tokens, and secrets
- Logs support operational monitoring and assurance

---

## 13. Testing considerations (design-level)

- Unit tests:
    - application services
    - business rules
    - search query construction
- Integration tests:
    - repositories against Postgres (where feasible)
    - Lucene indexing and querying (where feasible)
- End-to-end tests:
    - critical user journeys via Playwright

---

## 14. Trust boundaries (assurance)

- User input is untrusted at Web boundary
- Claims from DfE Sign-in are untrusted and validated
- PostgreSQL is the source of truth
- Lucene is derived data for performance only

---

## 15. References

- High Level Design: `/docs/architecture/hld.md`
- ERD (data model): `/docs/data/erd.md`
- ADRs (decisions): `/docs/adrs/`
- Developer handbook: `/docs/developers/`
