# Search (Postgres full text search) – Developer Guide

School search is backed by Postgres full text search. See [ADR-017](../adrs/017-fts-in-postgres.md).

## Rules

- Controllers must not know how search is implemented; they use `ISchoolSearchService`.
- Search SQL lives behind `IEstablishmentRepository.SearchEstablishmentsAsync` (Postgres implementation in `SAPSec.Infrastructure/Postgres`).
- `PostgresSchoolSearchIndexReader` (`SAPSec.Infrastructure/PostgresSearch`) implements `ISchoolSearchIndexReader` and adds `*word*` highlighting.

## How it works

- The SAPData pipeline builds `v_establishment_search` (`"URN"`, `"SearchFTS"`): a `tsvector` (config `simple`, so no stemming or stop words) over establishment name, street and postcode, with a GIN index.
- Synonyms (`st`/`saint`, `cofe`/`church of england`, `rd`/`road`, ...) live in `public.thesaurus_synonyms`, seeded from `SearchSynonymGroups` in `SAPData/Program.cs`. `normalize_text()` expands them, and is applied to both the indexed text and the query so they agree.
- Queries: every token must match; the last token is a prefix match (typeahead).
- Only establishments that pass `CanIndexForSearch()` are returned; the phase rule is mirrored in SQL.

## Changing search

- Change synonyms in `SAPData/Program.cs`, and keep `InMemoryEstablishmentSearch` (SAPSec.Core, used by the JSON/in-memory repositories in tests and UI fixtures) in step.
- Changes to indexed fields mean editing `GenerateEstablishmentSearchView` in `SAPData/GenerateViews.cs` and re-running the pipeline. No app-side rebuild is needed.
- Deploy order: run the data pipeline first (it creates `v_establishment_search`), then the app.
