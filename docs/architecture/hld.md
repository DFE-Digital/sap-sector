# High Level Design (HLD) – SAP Sector

## 1. Purpose

This document provides a **High Level Design (HLD)** for the SAP Sector service.

It exists to:
- support **assurance and assessment**
- clearly document the service for **developers, newcomers, and external reviewers**
- explain **all major service components, user types, data types, and how they interact**

For deeper technical detail (class/module/interface level), see the Low Level Design (LLD).

---

## 2. Scope

### In scope
- User types and access patterns
- Service components and responsibilities
- Data stores and search subsystem
- External integrations (DfE Sign-in)
- High-level interaction flows (search, view details, compare)
- High-level operational and security considerations

### Out of scope
- Method/class-level design, internal module structure (LLD)
- Full database schema (ERD)

---

## 3. Service overview

SAP Sector is a web application that enables schools and local authorities to:
- search for schools
- view school details
- compare similar schools

The service is implemented as an ASP.NET Core MVC application with:
- Single Sign-On using **DfE Sign-in**
- **PostgreSQL** as the authoritative data store
- **Lucene** for fast full-text search and filtering
- GOV.UK / DfE frontend assets (npm-managed and served from `wwwroot`)

---

## 4. Users and user types

### 4.1 End user types

1. **Unauthenticated user (if supported)**
    - Accesses public pages and journeys that do not require sign-in.

2. **Authenticated user (DfE Sign-in)**
    - Typical users: school staff and local authority staff.
    - Access to features is determined by claims/roles/policies.

### 4.2 Operational user types

3. **Service operator / Support**
    - Views health endpoints and operational metrics/logs.
    - Investigates incidents and monitors uptime and performance.
    - May perform operational actions (for example, initiating data refresh/reindex if implemented).

---

## 5. Data and information types

### 5.1 Authoritative service data (PostgreSQL)

PostgreSQL is the system of record for persisted data used by the service. Typical categories include:
- School identity and attributes (e.g., URN, name, address, LA)
- Attributes required for filtering and comparison (phase/type/status)
- Metrics or performance-related values (if stored)
- Relationships used for comparisons (if stored rather than derived)

> The full schema is documented separately in the ERD.

### 5.2 Search/index data (Lucene)

Lucene stores **derived** search data optimised for:
- full-text search
- fast filtering
- paging/ordering results

Lucene is not authoritative. It is built from authoritative sources (commonly PostgreSQL and/or imported datasets).

### 5.3 Identity and access data (DfE Sign-in)

DfE Sign-in provides:
- authentication identity
- claims/roles/organisation context (as configured)

This information is used for route protection and feature access decisions.

---

## 6. Service components

### 6.1 Web Application (ASP.NET Core MVC)

Responsibilities:
- Handles HTTP requests and responses
- Input validation and routing
- Authentication/authorisation enforcement
- Rendering Razor views
- Mapping application results into ViewModels

Key characteristics:
- Thin controllers (orchestration only)
- No direct database or Lucene access from controllers

---

### 6.2 Application/Service Layer (Use-cases)

Responsibilities:
- Implements user journeys as **use-cases**, for example:
    - Search schools
    - View school details
    - Compare similar schools
- Coordinates repository access and search access
- Enforces business rules and validation
- Returns application-level DTOs/results to the Web layer

---

### 6.3 Infrastructure Layer

Responsibilities:
- PostgreSQL access via repository implementations
- Lucene indexing and searching implementation
- External adapters and technical services (where applicable)
- Configuration binding and environment integration

Key characteristic:
- Owns all persistence and Lucene-specific concerns

---

### 6.4 Data Stores

- **PostgreSQL**: authoritative persistent store
- **Lucene index**: derived, optimised store for search

---

## 7. Interactions and key flows (high level)

### 7.1 Search schools (primary journey)

1. User submits a search term and optional filters
2. Web layer validates input and calls the Search use-case
3. Application service builds a safe search request
4. Infrastructure queries Lucene and returns paged results
5. Web layer maps results to a ViewModel and renders the page

Systems involved:
- Web (MVC)
- Application services
- Lucene

---

### 7.2 View school details

1. User selects a school from search results
2. Web calls the “Get school details” use-case
3. Application service queries PostgreSQL repository (authoritative)
4. Details are mapped and rendered

Systems involved:
- Web (MVC)
- Application services
- PostgreSQL

---

### 7.3 Compare similar schools

1. User selects a school and chooses “compare”
2. Web calls the Compare use-case
3. Application service determines a set of similar schools:
    - may be derived via rules and/or Lucene and/or stored relationships (depending on implementation)
4. Application service loads required details from PostgreSQL
5. Web renders comparison view

Systems involved:
- Web (MVC)
- Application services
- PostgreSQL and possibly Lucene

---

### 7.4 Sign-in and protected routes

1. User requests a protected feature/page
2. Web layer redirects to DfE Sign-in for authentication
3. User returns with identity/claims
4. Authorisation rules/policies determine access

Systems involved:
- Web (MVC)
- DfE Sign-in

---

## 8. Diagrams

### 8.1 System context

WIP


### 8.2 Component view

WIP

## 9. Non-functional considerations (assurance)

### 9.1 Security
- DfE Sign-in for authentication
- Explicit authorisation controls for protected routes
- Strict Content Security Policy (CSP) approach
- No secrets in source control
- Avoid logging sensitive data, tokens, or unnecessary PII

### 9.2 Performance
- Search requests are paged and limited
- PostgreSQL is used as source of truth for detail pages
- Indexing strategy ensures Lucene remains performant

### 9.3 Availability and monitoring

- Health endpoints provide basic and detailed status:
    - /healthcheck
    - /health
- Health responses must not expose sensitive infrastructure detail

## 10. Assumptions and constraints

- The service is maintained as a public repository and must follow secure development practices.
- Lucene is a derived data store and may require reindexing after schema or data model changes.
- PostgreSQL is authoritative for persisted entities.

## 11. References

- Developer handbook: /docs/developers/
- ADRs (decisions and rationale): /docs/adrs/
- ERD (data model): /docs/data/erd.md
- Low Level Design (LLD): /docs/architecture/lld.md