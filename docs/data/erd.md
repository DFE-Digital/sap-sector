# Entity Relationship Diagram (ERD) – SAP Sector

## 1. Purpose

This document describes the **logical PostgreSQL data model** for the SAP Sector service, including:
- the main data entities
- relationships (PK/FK)
- how data supports user journeys (search, view, compare)

This is provided for:
- assurance and assessment
- onboarding (new developers and reviewers)
- clear understanding of how data is stored and accessed

> **Source of truth:** the database schema/migrations.  
> Update this ERD whenever schema changes.

---

## 2. Data stores and scope

### PostgreSQL (authoritative)
PostgreSQL stores the authoritative data used by the service (school records, attributes, metrics, etc.).

### Lucene (derived, not authoritative)
Lucene holds a derived search index built from authoritative sources (typically PostgreSQL and/or imported datasets).  
Lucene is documented in developer docs and the HLD/LLD; it is not modelled as an ERD entity because it is not relational.

---

## 3. Core entities (logical model)

The service domain suggests these core logical entities:

- **LocalAuthority**
- **School**
- **SchoolCharacteristic** (key/value attributes used for display/filter/compare)
- **SchoolMetric** (numeric/time-series values used for compare)
- **SimilarSchool** (optional: stored similarity relationships)

If your implementation does not store some of these (e.g., similarity is computed dynamically), remove those entities from this ERD.

---

## 4. Logical ERD (Mermaid)

> WIP.

---

## 5. Relationship definitions

### LOCAL_AUTHORITY → SCHOOL (1-to-many)
- One local authority oversees many schools.
- A school belongs to one local authority (if known).

### SCHOOL → SCHOOL_CHARACTERISTIC (1-to-many)
- A school has multiple characteristics (e.g., governance, admissions, etc.).
- Characteristics should be stable, queryable, and safe to display.

### SCHOOL → SCHOOL_METRIC (1-to-many)
- A school has multiple metrics.
- Metrics may be time-bound (academic year) or current.

### Similar schools (logical relationship)
The concept of “similar schools” is represented logically rather than as a direct relational join in the Mermaid ERD.

If similarity is persisted:
- a join table (e.g. `SIMILAR_SCHOOL`) stores relationships between two schools
- both references point to `SCHOOL.urn`
- this relationship is many-to-many and self-referencing

If similarity is derived:
- similarity is calculated dynamically using rules and/or search
- no persistent relationship table exists

This approach avoids circular dependencies in the ERD and reflects the implementation accurately.

---

## 6. Data used by key user journeys

### 6.1 Search schools
- Primary search is served by Lucene (derived index).
- Filters typically rely on DB-backed attributes such as:
    - phase
    - school_type
    - local_authority_id
    - status / is_open

When a user selects a result, the service should load authoritative details from PostgreSQL.

### 6.2 View school details
- Fetch from `SCHOOL` and any related tables needed for display:
    - characteristics
    - metrics
    - local authority

### 6.3 Compare similar schools
- Identify a comparison set:
    - from a similarity join table (if stored), or
    - derived dynamically via rules/search
- Load authoritative details for all schools being compared from PostgreSQL.

---

## 7. Indexing and constraints (assurance)

Document the indexes that support performance and integrity.

### Suggested constraints
- WIP



### Suggested indexes (examples)
- WIP

If similarity is stored:
- WIP

---

## 8. Consistency with Lucene (derived data)

Document how the Lucene index stays consistent with PostgreSQL:

- **Indexed fields:** list the fields that are copied/derived into Lucene
- **Index trigger:** when indexing occurs (on import, scheduled job, manual run)
- **Reindex strategy:** how schema changes or data updates are handled

Example (update to reality):
- School name and postcode are indexed for text search
- Phase/type/status are indexed for filtering
- A rebuild is required when index schema changes

---

## 9. Change control

Any schema change must update:
- database migrations/schema
- this ERD (`docs/data/erd.md`)
- dependent repository queries and mapping
- Lucene indexing schema if indexed fields change
- tests (unit/integration) covering impacted flows
