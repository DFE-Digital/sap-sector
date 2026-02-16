# Search (Lucene) – Developer Guide

---

## Rules

- Controllers must not know Lucene exists
- All Lucene logic lives in Infrastructure
- Never execute raw user input

---

## Indexing & querying

- Schema must be explicit
- Schema changes require reindexing
- Query logic must be testable

For architectural decisions, see:
- `/docs/adrs/010-software-architecture.md`
