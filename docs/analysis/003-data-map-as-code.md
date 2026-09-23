# Data Map As Code: Problem Statement And Planned Work

## Purpose

This document describes the problems with the current spreadsheet-style data map (`datamap.csv`), the proposed move to a code-defined metric catalogue with an API-style measure service, and the planned work broken into user stories.

It complements [002-data-pipeline-pain-points.md](002-data-pipeline-pain-points.md), which covers the pipeline workflow itself.

---

## Problem Statement

The SAP data pipeline is driven by `SAPData/DataMap/datamap.csv`, a 44-column spreadsheet maintained by hand in Excel. Every combination of metric × breakdown × year × range is written out as its own row, so the file has grown to ~4,950 rows. Adding a new year of data, a new metric or a new breakdown means editing hundreds of cells by hand. Mistakes are only discovered when the full pipeline runs, and changes cannot be meaningfully reviewed because Excel rewrites the whole file on save.

On the consuming side, the website reads very wide generated DTOs (up to ~490 string properties) and selects measures by hard-coded column names such as `Attainment8_Tot_Est_Current_Num`, so every new metric requires regenerating DTOs and writing more mapping code.

### Evidence

| Issue | Detail |
|---|---|
| Size and repetition | ~4,950 rows, 44 columns; one metric (e.g. Attainment8) repeats across 7 breakdowns × 3 years × 3 ranges |
| Dead configuration | ~1,990 rows marked `IgnoreMapping = Y` |
| Identity problems | ~280 rows with no `REF`; duplicate REFs (e.g. `G_ADD`, `24_KS4SE_BIO_49_BOY_ENG`) |
| Inconsistent values | `RecordFilterBy` uses `URN`, `urn` and `Unique Reference Number (URN)` for the same concept |
| Awkward structure | Filters stored as `Filter`/`FilterValue` … `Filter9`/`Filter9Value`; `+` inside a value means OR |
| Duplicated source of truth | Two copies of `datamap.csv` (`SAPData/DataMap` and `Data/SAPSec.PrimaryJsonFileGenerator/DataMap`) which have already diverged |
| Unreviewable changes | The latest data map commit shows +4,948 / −1,865 lines because Excel rewrote the file |
| Lost typing | ~2,780 rows declare `DataType = double`, but `GenerateViews.BuildValueExpression` only handles `int`, `percentage` and `numeric`, so these values are emitted as text and all generated DTO properties are `string` |
| Wide consumer model | Core measures select values through lambdas per column, three at a time (Current / Previous / Previous2) |

### Impact of a new year of data (today)

When a new year of data is published, each dataset has to be rolled manually:

| Dataset | Rows per year |
|---|---|
| KS4 Performance | ~449 |
| KS2 Performance | ~266 |
| KS4 Destinations | ~128 |
| Pupil Absence | ~90 |

For KS4 Performance alone this means relabelling ~1,300 rows (Current → Previous → Previous2), removing ~430 old rows and adding ~450 new rows, each with the correct file name, REF year prefix and `time_period` filter value. ~2,700 rows contain a year inside a filter value.

---

## Goals

- Define the data map in code, so metrics, breakdowns and years are declared once and expanded automatically
- Make a new year of data a small, reviewable change (a few lines per dataset)
- Catch data map mistakes at build or unit-test time, not during a pipeline run
- Have one source of truth for the data map
- Keep numeric values numeric from raw table through to DTO
- Give website code an API-style way to request measures (metric, school/LA/England, breakdown, period) instead of selecting columns by name

## Non-Goals

- Splitting or rewriting the GitHub Actions workflow YAML, maintenance page switching or ETL retry logic (covered by [002](002-data-pipeline-pain-points.md))
- Changing the raw table load process (`GenerateRawTables`)
- Changing any values shown on the website (phases 1 and 2 must produce identical output)

---

## Proposed Solution

### Phase 1 and 2: Metric catalogue in code

Replace `datamap.csv` with a typed C# catalogue. Each metric is declared once; breakdowns, ranges and year sources are declared separately and combined by a builder.

```csharp
Metric("Attainment8", "Attainment 8 score")
    .Numeric()
    .Breakdowns(Breakdown.Standard)           // Tot, Boy, Grl, Dis, NDi, EAL, NMo
    .For(Range.Establishment, Range.LA, Range.England)
    .Year(Current,   Ees.Ks4Schools("202425_performance_tables_schools_final")
                        .Field("attainment8_average").KeyedBy("school_urn")
                        .Where("time_period", "202425")
                        .BreakdownColumn("breakdown"))
    .Year(Previous2, Cscp.Ks4Final("2022-2023_england_ks4final")
                        .KeyedBy("URN")
                        .FieldPerBreakdown(b => $"ATT8SCR_{b.CscpSuffix}"));
```

The catalogue expands to the existing `DataMapRow` list, so `GenerateViews`, `GenerateSimilarSchoolsViews` and the generated SQL do not need to change. A golden test proves the catalogue produces byte-identical SQL to the current CSV before the CSV is removed.

A new year of data becomes:

```csharp
Ks4Performance.Years(
    Current:   Ees.Ks4Schools("202526_performance_tables_schools_final").Period("202526"),
    Previous:  Ees.Ks4Schools("202425_performance_tables_schools_final").Period("202425"),
    Previous2: Ees.Ks4Schools("202324_performance_tables_schools_final").Period("202324"));
```

If a source file changes shape (renamed columns, new breakdown labels), only that year's source adapter needs updating.

### Phase 3: API-style measure access

Add a long-format measures view alongside the existing wide views:

```
v_measures (scope, entity_id, metric, breakdown, period, value numeric)
```

and a typed service over it:

```csharp
var att8 = await measures.GetAsync(Metrics.Attainment8, School(urn), Breakdown.Standard, Periods.Last3);
// → IReadOnlyList<MeasureValue>(Scope, Breakdown, Period, decimal? Value)
```

New metrics then reach the website with a catalogue entry only, with no DTO regeneration or new column mapping. These services can later be exposed as HTTP endpoints (e.g. `/api/schools/{urn}/measures/attainment8`) if required.

---

## Planned Work (User Stories)

### Epic: Data map as code

#### Story 1: Metric catalogue model and builder

**As a** developer maintaining the data pipeline
**I want** metrics, breakdowns, ranges and year sources defined in C#
**So that** each metric is declared once instead of repeated across hundreds of spreadsheet rows

Acceptance criteria:
- Catalogue types exist for Metric, Breakdown, Range, Period and Source (EES, CSCP, GIAS)
- The builder expands a catalogue into `IReadOnlyList<DataMapRow>`
- Unit tests cover expansion: breakdown × year × range, filters, OR values, data types, key columns
- No change to pipeline behaviour (catalogue not yet wired in)

#### Story 2: CSV-to-catalogue converter and golden SQL test (KS4 Performance pilot)

**As a** developer
**I want** a converter that generates catalogue code from `datamap.csv`, and a test proving identical output
**So that** the migration is safe and does not depend on hand re-typing

Acceptance criteria:
- A one-off converter produces catalogue code for `Type = KS4_Performance`
- Golden test: SQL generated from the catalogue is identical to SQL generated from the CSV for all KS4 Performance views (`v_establishment_performance`, `v_la_performance`, `v_england_performance`, `v_establishment_subject_entries`, `v_la_subject_entries`)
- `Program.cs` uses the catalogue for KS4 Performance and the CSV for everything else
- Generated JSON and DTOs are unchanged

#### Story 3: Migrate remaining datasets and remove both CSVs

**As a** developer
**I want** every dataset defined in the catalogue with a single source of truth
**So that** the two diverged `datamap.csv` files can be deleted

Acceptance criteria:
- KS2 Performance, KS4 Destinations, Pupil Absence, Workforce, Establishment (GIAS), Links, Email and Similar Schools are migrated
- Golden SQL test passes for all views
- `SAPSec.PrimaryJsonFileGenerator` uses the shared catalogue
- Both `datamap.csv` files and `IgnoreMapping` rows are removed
- README and runbook updated

#### Story 4: Catalogue validation tests

**As a** developer
**I want** data map mistakes caught in unit tests
**So that** they are found in a PR, not during a pipeline run

Acceptance criteria:
- Tests fail on: duplicate REF or property name, missing REF, a metric without a source for each declared year, a filter without a value, an unknown data type, inconsistent key column names
- Optional: when sample source headers are available, referenced fields are checked against them
- Tests run in CI on every PR

#### Story 5: Keep numeric values numeric

**As a** developer
**I want** `double`/numeric measures stored and exposed as numbers
**So that** the website does not parse strings and comparisons and formatting are reliable

Acceptance criteria:
- `BuildValueExpression` (or its replacement) casts numeric types via `clean_numeric`
- Generated DTO properties for numeric measures are `decimal?`
- Core measure code updated to use numeric values; no change to values shown on the website (verified by existing UI/integration tests)

#### Story 6: Derive raw-table rebuild list from the catalogue

**As a** pipeline operator
**I want** the pipeline to work out which raw tables and views are affected by a change
**So that** `raw_tables_to_rebuild.*.txt` does not need to be maintained by hand

Acceptance criteria:
- The catalogue exposes view → source file dependencies
- The rebuild list is derived from changed source files (hash comparison) and catalogue dependencies
- `RebuildAllRawTables` override still works
- Existing rebuild-list files removed or kept only as an override

#### Story 7: New year rollover process

**As a** developer
**I want** a documented, small, reviewable process for adding a new year of data
**So that** annual updates are quick and low-risk

Acceptance criteria:
- Runbook section: "Adding a new year of data"
- Rollover of one dataset is a change to its year sources only
- Validation tests confirm every metric has all declared years
- Demonstrated with a dry run (e.g. using a prior year) and golden output reviewed

### Epic: API-style measure access

#### Story 8: Long-format measures view and measure service (Attainment 8 pilot)

**As a** website developer
**I want** to request measures by metric, entity, breakdown and period
**So that** I do not select generated columns by name

Acceptance criteria:
- `v_measures (scope, entity_id, metric, breakdown, period, value)` generated from the catalogue alongside existing wide views
- `IMeasureService.GetAsync(metric, entity, breakdowns, periods)` implemented in Infrastructure (Postgres and JSON test data)
- Attainment 8 headline measures moved to the new service with identical output
- Existing wide views and DTOs remain in place

#### Story 9: Migrate Core measures to the measure service

**As a** website developer
**I want** all measure pages to use the measure service
**So that** new metrics reach the website without DTO regeneration

Acceptance criteria:
- KS4 headline, KS4 subject, KS2, Destinations and Absence measures moved over, one PR per area
- Wide DTOs and views removed once unused
- Adding a new metric requires only a catalogue entry and the page/view change

---

## Suggested Order

1. Story 1: Catalogue model and builder
2. Story 2: KS4 Performance pilot with golden test
3. Story 4: Validation tests (can start alongside Story 2)
4. Story 3: Migrate remaining datasets, remove CSVs
5. Story 7: Rollover process (ideally before the next data release)
6. Story 5: Numeric typing
7. Story 6: Derived rebuild list
8. Stories 8 and 9: API-style measure access

---

## Risks And Mitigations

| Risk | Mitigation |
|---|---|
| Migration changes website values | Golden SQL test must pass before any CSV is removed; phases 1–2 change no output |
| Non-developers can no longer edit the data map in Excel | Catalogue reads close to plain English; optionally export a read-only CSV/HTML report of the catalogue for data team review |
| Source file layouts differ by year and publisher | Per-year source adapters (EES, CSCP, GIAS) isolate differences |
| Numeric typing (Story 5) changes DTO types | Separate story, after migration, with its own regression testing |
| Long-format view performance | Pilot with Attainment 8, index on `(scope, entity_id, metric)`, compare with wide view before wider rollout |

---

## Data Issues Found And Fixed During Migration

Found while converting KS4 Performance (Story 2) and fixed in the catalogue. The golden test lists these as the only intended differences from datamap.csv.

| Dataset | Issue in datamap.csv | Fix | Effect on the website |
|---|---|---|---|
| KS4 Performance | `Prog8_Avg_LA_Previous2_Num` was mapped three times; the generator used the first, which filtered only `sex = Total` for 2024-25, so `MAX` ran across every breakdown topic | `Prog8_Tot_LA_{Current,Previous,Previous2}_Num` filtered to `breakdown_topic = Total` | LA Progress 8 is available for each year (not currently displayed) |
| KS4 Performance | `EngMaths59_Mob_Eng_*` (England, mobile pupils, grade 5+) read `engmath_94_*` | Reads `engmath_95_*` | e.g. 2024-25 national shows 23.4% instead of the grade 4+ figure 39.5% |
| KS4 Performance | `EngLang79`, `EngLit79`, `Maths79` for schools in 2023-24 filtered `grade = 7 OR 8 OR 9`, values that don't exist in the file | Filter `grade = 9 to 7` as in other years | ~3,650 schools now show a 2023-24 value instead of blank |

Verified with a full local pipeline run (Postgres 16, real source files): across `v_establishment_performance`, `v_la_performance` and `v_england_performance`, these 16 columns are the only differences from the datamap.csv-driven views.

Related observation for Story 5: `double` values are stored as text, so `MAX` compares them as strings whenever a filter matches more than one row.

---

## Success Measures

- A new year of data for one dataset is a change of under ~10 lines, reviewable in a PR
- Data map errors are caught by unit tests before a pipeline run
- One data map source of truth (zero CSV copies)
- Numeric measures typed as numbers end-to-end
- Adding a new metric to the website requires no DTO regeneration (after Epic 2)
