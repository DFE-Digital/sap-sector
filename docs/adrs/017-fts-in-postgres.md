# 017 - Architecture Decision Record: Full text search in Postgres

- **Status:** Proposed
- **Deciders:** TBC (team review)
- **Date:** 2026-11-05

## Context and Problem Statement

- School search used Lucene.NET with an in-memory (RAMDirectory) index built by each web pod at startup.
- That adds startup time, a per-replica index, and a separate search component to maintain, even though Postgres is already our primary data store.
- SAP Public made the same move (see sap-public ADR-020, PR #346): a Postgres FTS index built by the data pipeline.

## Considered Options

1. Keep Lucene.
2. Postgres full text search, built in the data pipeline (chosen).

## Decision Outcome

Replace Lucene with Postgres FTS, reusing SAP Public's approach (pipeline-built `tsvector` with GIN index, synonym table, `normalize_text()`), adapted to keep current behaviour:

- Index covers name, street and postcode (`simple` config: no stemming, no stop words, as with Lucene).
- Last token is a prefix match for typeahead; all other tokens are required.
- Same synonym groups as the Lucene map, expanded at index and query time. Multi-word synonyms (e.g. "church of england") work, unlike the word-by-word approach in sap-public.
- `*word*` highlighting is done in the app.
- PostGIS is not adopted: sector converts easting/northing in C#, so no infrastructure changes are needed.
- The search document is a separate materialized view (`v_establishment_search`) so the `Establishment` DTO and JSON export are unchanged.

## Consequences

- Positive: no startup index build, no per-pod index, one less dependency, and the search data is updated by the pipeline.
- Negative: the app now depends on the pipeline having created `v_establishment_search` (run the pipeline before deploying the app); search performance depends on the database and is not yet benchmarked; ranking is `ts_rank_cd` rather than Lucene scoring (results are re-sorted by name by `SchoolSearchService` anyway).
- JSON/in-memory repositories (used by UI, integration and accessibility tests) use a LINQ stand-in, `InMemoryEstablishmentSearch`, which must be kept in step with the SQL synonyms.
