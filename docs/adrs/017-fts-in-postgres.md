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

## Learnings From SAP Public (team feedback)

- **Very common words are slow.** SAP Public found that searching for "school" performed badly, and removed that case. Our query turns the last token into a prefix match (`school:*`) that matches most rows, which then have to be ranked and sorted (the service asks for up to 1000). This has not been tested here. Planned mitigation: ignore very common words (school, academy, primary, high, and similar) when other words are present, handle a query made only of such words cheaply (minimum length or a smaller limit), and benchmark on real data before release.
- **PostGIS needs installing in the pipeline and on every developer machine** (including re-installing Postgres locally). We chose not to adopt it, so we avoid that cost.
- **Backup/restore quirk on first publish.** SAP Public hit a problem with their backup-restore deployment cycle when search was first published (resolved, details not remembered). Our change adds a function, a synonym table and a materialized view that depends on them. Check `.github/workflows/backup-db.yml` and do a trial restore in test before production.

## Follow-ups

- Talk to Spencer O'Hegarty (GIAS replacement), who is building a reusable search-against-Postgres library, to decide whether to adopt it or stay compatible with it. Hari Dupati has offered to book a call and has a spike write-up explaining how SAP Public's search works.
- Ask Dan Murfitt and Cath Lawlor / Naomi Todd (who built the SAP Public version) about the backup-restore issue and any other quirks.
- Benchmark typeahead and search latency, including common-word queries, before moving this ADR from Proposed to Accepted.
