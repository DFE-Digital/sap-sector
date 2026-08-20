# SAP Sector - Low-Level Design (LLD)

**Repository:** `DFE-Digital/sap-sector`
**Author:** Hari Dupati
**Last updated:** 2026-08-20
**Status:** Draft
**Relates to:** `docs/architecture/high-level-design.md`

---

## Contents

1. [Purpose and scope](#1-purpose-and-scope)
2. [Route map](#2-route-map)
3. [Authentication and authorisation](#3-authentication-and-authorisation)
4. [Search and phase routing](#4-search-and-phase-routing)
5. [Feature flag behaviour](#5-feature-flag-behaviour)
6. [Request flows](#6-request-flows)
7. [C4 Level 3, component view](#7-c4-level-3-component-view)
8. [Controllers](#8-controllers)
9. [Measure components and ViewModels](#9-measure-components-and-viewmodels)
10. [Use cases and business rules](#10-use-cases-and-business-rules)
11. [Performance measures domain model](#11-performance-measures-domain-model)
12. [Repositories and data sources](#12-repositories-and-data-sources)
13. [Error handling](#13-error-handling)
14. [School layout and side navigation](#14-school-layout-and-side-navigation)
15. [Data pipeline dependency](#15-data-pipeline-dependency)
16. [Test coverage](#16-test-coverage)
17. [Phase comparison and refactoring notes](#17-phase-comparison-and-refactoring-notes)
18. [Class diagrams](#18-class-diagrams)

---

## 1. Purpose and scope

This document covers the low-level design of the SAP Sector service. It describes the layered architecture, authentication, the search pipeline, middleware, error handling and testing patterns.

For the system boundary, data flows and C4 levels 1 and 2, see the [High-Level Design](./high-level-design.md).

---

## 2. Route map

Route helpers: `Routes.PrimarySchool(urn)` and `Routes.SecondarySchool(urn)` return typed instances. After search, `SchoolSearchController.BuildSchoolUrl()` picks the correct base path using `PhaseOfEducationValues.IsPrimaryOrAllThrough(phase)`.

### Primary, `/school/primary/{urn}`

| Route                                                                    | Action                     |
| ------------------------------------------------------------------------ | -------------------------- |
| `/school/primary/{urn}`                                                  | `Index`, overview          |
| `/school/primary/{urn}/ks2`                                              | `Ks2PerformanceMeasures`   |
| `/school/primary/{urn}/attendance`                                       | `Attendance`               |
| `/school/primary/{urn}/view-similar-schools`                             | `ViewSimilarSchools`       |
| `/school/primary/{urn}/school-details`                                   | `SchoolDetails`            |
| `/school/primary/{urn}/what-is-a-similar-school`                         | `WhatIsASimilarSchool`     |
| `/school/primary/{urn}/view-similar-schools/{similarUrn}`                | Comparison `Similarity`    |
| `/school/primary/{urn}/view-similar-schools/{similarUrn}/ks2`            | Comparison `Ks2`           |
| `/school/primary/{urn}/view-similar-schools/{similarUrn}/attendance`     | Comparison `Attendance`    |
| `/school/primary/{urn}/view-similar-schools/{similarUrn}/school-details` | Comparison `SchoolDetails` |

### Secondary, `/school/secondary/{urn}`

| Route                                                                             | Action                           |
| --------------------------------------------------------------------------------- | -------------------------------- |
| `/school/secondary/{urn}`                                                         | `Index`, overview                |
| `/school/secondary/{urn}/ks4-headline-measures`                                   | `Ks4HeadlineMeasures`            |
| `/school/secondary/{urn}/ks4-headline-measures/data`                              | `Ks4HeadlineMeasuresData`        |
| `/school/secondary/{urn}/ks4-core-subjects`                                       | `Ks4CoreSubjects`                |
| `/school/secondary/{urn}/ks4-core-subjects/data`                                  | `Ks4CoreSubjectsData`            |
| `/school/secondary/{urn}/attendance`                                              | `Attendance`                     |
| `/school/secondary/{urn}/attendance-data`                                         | `AttendanceData` (JSON)          |
| `/school/secondary/{urn}/ks4-destinations/data`                                   | `Ks4DestinationsData` (JSON)     |
| `/school/secondary/{urn}/view-similar-schools`                                    | `ViewSimilarSchools`             |
| `/school/secondary/{urn}/school-details`                                          | `SchoolDetails`                  |
| `/school/secondary/{urn}/what-is-a-similar-school`                                | `WhatIsASimilarSchool`           |
| `/school/secondary/{urn}/view-similar-schools/{similarUrn}`                       | Comparison `Similarity`          |
| `/school/secondary/{urn}/view-similar-schools/{similarUrn}/ks4-headline-measures` | Comparison `Ks4HeadlineMeasures` |
| `/school/secondary/{urn}/view-similar-schools/{similarUrn}/ks4-core-subjects`     | Comparison `Ks4CoreSubjects`     |
| `/school/secondary/{urn}/view-similar-schools/{similarUrn}/attendance`            | Comparison `Attendance`          |
| `/school/secondary/{urn}/view-similar-schools/{similarUrn}/school-details`        | Comparison `SchoolDetails`       |

The `/data` routes are legacy JSON endpoints used by the older secondary pages. They are being removed as secondary moves onto the `Measure` pattern.

---

## 3. Authentication and authorisation

Authentication uses the OIDC Authorization Code flow through `Microsoft.AspNetCore.Authentication.OpenIdConnect` against DfE Sign-in (DSI). See `docs/adrs/009-authentication-provider.md`.

Every controller action across both phases carries `[Authorize]`, which enforces the global DSI policy. Users without an organisation claim, meaning they are not linked to a school or trust in DSI, are redirected to `/error/403` with a prompt to link their DSI account.

Key config sits in `appsettings.json` under `"DsiConfiguration"`: `Authority`, `ClientId`, `ClientSecret`, `MetadataAddress` and `TokenExpiryMinutes` (default 60). The cookie is named `SAPSec.Auth` and uses sliding expiration, HttpOnly and SecureAlways.

OIDC event handlers in `DsiAuthenticationHandler`:

| Event                                        | Purpose                                    |
| -------------------------------------------- | ------------------------------------------ |
| `OnTokenValidated`                           | Enriches claims and sets organisation context |
| `OnRemoteFailure` and `OnAuthenticationFailed` | Redirects to `/error` and logs             |
| `OnSignedOutCallbackRedirect`                | Redirects to `/` after sign-out            |

For UI and integration tests, `AutoAuthenticationHandler` bypasses DSI and injects a fixed test identity with `sub`, `email` and `organisation` claims.

---

## 4. Search and phase routing

`SchoolSearchService` checks the `EnablePrimarySchools` flag before including primary and all-through schools in results. The eligibility logic sits in `EstablishmentExtensions.CanSearch(establishment, primaryEnabled)`:

- phase ID `2` (Primary) or `7` (All-through) are included only if the flag is on
- phase ID `4` (Secondary) is always included
- establishment status `Closed` or `ProposedToOpen` is excluded either way

Once a match is found, `BuildSchoolUrl()` routes by phase:

```
PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName)
    ? Routes.PrimarySchool(urn).Overview   // /school/primary/{urn}
    : Routes.SecondarySchool(urn).Overview // /school/secondary/{urn}
```

The Lucene index stores `urn`, `establishmentName`, `street` and `postcode`. The last query token uses `PrefixQuery` for typeahead, and middle tokens use `TermQuery` (MUST). Phrase and exact-name boosts are applied. Primary schools are always indexed regardless of the feature flag, because `CanIndexForSearch()` is flag-unaware. See `docs/developers/search-lucene.md` for the full detail.

---

## 5. Feature flag behaviour

The flag is `FeatureFlags.EnablePrimarySchools = "EnablePrimarySchools"`, managed through `Microsoft.FeatureManagement`. Secondary has no equivalent flag.

| Component                                           | When flag is off                     |
| --------------------------------------------------- | ------------------------------------ |
| `[RequireFeatureFlagFilter]` on primary controllers | HTTP 404 for all primary pages       |
| `SchoolSearchService.SearchAsync()`                 | Primary and all-through schools excluded |
| `SchoolSearchService.SearchByNumberAsync()`         | Returns `null` for primary URNs      |
| Lucene index                                        | Primary schools remain indexed       |

Integration tests verify this through `Fixture.FeatureFlagService.Override(FeatureFlags.EnablePrimarySchools, false)`.

---

## 6. Request flows

### 6.1 KS2 performance page

This traces `GET /school/primary/{urn}/ks2`, which is the most representative flow for the `Measure` pattern.

```
[Authorize]               → DSI cookie valid? proceed | absent → 401
[RequireFeatureFlag]      → flag on? proceed | off → 404
[RequireSchoolPhase]      → Primary/All-through URN? proceed
                          → Secondary URN? redirect to SecondarySchool(urn).Overview
                          → not found? → 404

SchoolController.Ks2PerformanceMeasures(urn)
  └─ filters = Request.Query.ToDictionary()
  └─ GetSchoolKs2PerformanceMeasuresUseCase.Execute(new(urn, filters))
       └─ PrimarySimilarSchoolsPerformanceDataProvider
            ├─ ISimilarSchoolsPrimaryRepository.GetGroupAsync(urn)
            │    → v_similar_schools_primary_groups
            ├─ IEstablishmentRepository.GetEstablishmentsAsync(urn + neighbourUrns)
            │    → v_establishment
            └─ IKs2PerformanceRepository.GetByUrnsAsync(allUrns)
                 → JsonKs2PerformanceRepository
                   (establishment_performance.json, la_performance.json,
                    england_performance.json)
       └─ Ks2PerformanceMeasures.*.ForSchool(currentSchool, similarSchools, filters)
            → 6x Measure (each containing ThreeYearAverage,
                          TopPerformers, YearByYear SubMeasures)

  └─ Ks2MeasuresPageViewModel { School, 6x MeasureViewModel.FromMeasure(...) }
  └─ View("Ks2PerformanceMeasures")
       → renders _Measure partial per measure
       → each _Measure uses <tabbed-view> tag helper for chart/table tabs
       → filter <select> elements use measure-filters.js for AJAX partial refresh
```

The KS4 flow is the same shape, with `PostgresKs4PerformanceRepository` reading from PostgreSQL views instead of JSON files.

### 6.2 Other journeys

The remaining journeys named in the HLD are covered elsewhere in this document rather than repeated as traces:

| Journey                          | Where it is described                                   |
| -------------------------------- | -------------------------------------------------------- |
| Search schools                   | Section 4, and the class diagram in section 18.1         |
| View school details              | Section 8, `SchoolDetails` actions                       |
| Compare schools                  | Section 8, comparison controllers, and section 10        |
| Similar schools                  | Section 10, `FindPrimarySimilarSchoolsUseCase`           |
| Authentication and access        | Section 3                                                |
| Operational health and deployment | Section 15, and the workflow files listed in the HLD    |

---

## 7. C4 Level 3, component view

This is the component view of the web application. The container view sits in the HLD.

```mermaid
flowchart TB
    user[Authenticated User]
    dsi[DfE Sign-in]

    subgraph web[Web layer]
        middleware[Security, session and auth middleware]
        filters[RequireFeatureFlag / RequireSchoolPhase filters]
        searchctl[SchoolSearchController]
        schoolctl[SchoolController - primary and secondary]
        compctl[SimilarSchoolsComparisonController]
        errctl[ErrorController]
        vm[ViewModels and Measure partials]
    end

    subgraph core[Core layer]
        usecases[Use cases - IUseCase]
        measures[Measure / SubMeasure domain]
        rules[Similar schools filters and sorting]
        interfaces[Repository and service interfaces]
    end

    subgraph infra[Infrastructure layer]
        pgrepos[Postgres repositories - Dapper]
        jsonrepo[Json KS2 repository - interim]
        searchindex[Lucene index reader]
    end

    pg[(PostgreSQL materialised views)]
    lucene[(Lucene index)]
    jsonfiles[Packaged KS2 JSON files]

    user --> middleware
    dsi --> middleware
    middleware --> filters
    filters --> searchctl
    filters --> schoolctl
    filters --> compctl
    schoolctl --> vm
    compctl --> vm

    searchctl --> usecases
    schoolctl --> usecases
    compctl --> usecases
    errctl --> vm

    usecases --> measures
    usecases --> rules
    usecases --> interfaces

    interfaces --> pgrepos
    interfaces --> jsonrepo
    interfaces --> searchindex

    pgrepos -->|read only| pg
    searchindex -->|read only| lucene
    jsonrepo --> jsonfiles
```

*Figure 1. C4 level 3, components inside the web application.*

The layering matters here. Controllers never talk to a repository directly. Use cases depend on interfaces defined in Core, and Infrastructure supplies the implementations. That is what keeps Postgres, Lucene and the JSON files out of the business logic.

---

## 8. Controllers

Both phases use the same structural approach: `[Authorize]`, `[RequireSchoolPhase]`, use-case injection, `PopulateViewData()` or `SetSchoolViewData()`, and `MeasureViewModel` mapping.

### Primary `SchoolController`, `[Route("school/primary/{urn}")]`

Additional filters: `[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]` and `[RequireSchoolPhase(ExpectedSchoolPhase.Primary)]`.

| Action                   | Route suffix                | Returns                                            |
| ------------------------ | --------------------------- | -------------------------------------------------- |
| `Index`                  | *(none)*                    | `SchoolInfoViewModel`                              |
| `Ks2PerformanceMeasures` | `/ks2`                      | `Ks2MeasuresPageViewModel` (6x `MeasureViewModel`) |
| `Attendance`             | `/attendance`               | `AttendanceMeasuresPageViewModel`                  |
| `ViewSimilarSchools`     | `/view-similar-schools`     | `PrimarySimilarSchoolsPageViewModel`               |
| `SchoolDetails`          | `/school-details`           | via `IRequestSchoolAccessor`                       |
| `WhatIsASimilarSchool`   | `/what-is-a-similar-school` | `SchoolInfoViewModel`                              |

### Secondary `SchoolController`, `[Route("school/secondary/{urn}")]`

Filter: `[RequireSchoolPhase(ExpectedSchoolPhase.Secondary)]`. No feature flag filter.

| Action                    | Route suffix                  | Returns                                      |
| ------------------------- | ----------------------------- | -------------------------------------------- |
| `Index`                   | *(none)*                      | `SchoolDetails` via `IRequestSchoolAccessor` |
| `Ks4HeadlineMeasures`     | `/ks4-headline-measures`      | `Ks4HeadlineMeasuresPageViewModel`           |
| `Ks4HeadlineMeasuresData` | `/ks4-headline-measures/data` | JSON, legacy                                 |
| `Ks4CoreSubjects`         | `/ks4-core-subjects`          | `Ks4CoreSubjectsPageViewModel`               |
| `Ks4CoreSubjectsData`     | `/ks4-core-subjects/data`     | JSON, legacy                                 |
| `Attendance`              | `/attendance`                 | `SchoolAttendancePageViewModel`              |
| `AttendanceData`          | `/attendance-data`            | JSON, legacy                                 |
| `SchoolDetails`           | `/school-details`             | via `IRequestSchoolAccessor`                 |
| `WhatIsASimilarSchool`    | `/what-is-a-similar-school`   | via `IRequestSchoolAccessor`                 |

### Comparison controllers

Both phases have a `SimilarSchoolsComparisonController`. Primary applies `[RequireSchoolPhase]` to both `urn` and `similarSchoolUrn`. Secondary applies it to the current school only.

|                  | Primary                                                   | Secondary                                                                   |
| ---------------- | --------------------------------------------------------- | --------------------------------------------------------------------------- |
| Base route       | `/school/primary/{urn}/view-similar-schools/{similarUrn}` | `/school/secondary/{urn}/view-similar-schools/{similarUrn}`                 |
| Comparison pages | Similarity, KS2, Attendance, SchoolDetails                | Similarity, KS4HeadlineMeasures, KS4CoreSubjects, Attendance, SchoolDetails |
| Measure pattern  | Fully implemented                                         | Being adopted, legacy bespoke fields still in places                        |

---

## 9. Measure components and ViewModels

A unified component model. It is fully in place for primary, and in place for the secondary school pages. The secondary comparison pages are still being brought across. All new measure work should follow this pattern.

### The `Measure` domain type

```
public record Measure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilter> Filters,
    IEnumerable<SubMeasure> SubMeasures);
```

Constructed through:

- `Measure.ForSchool(...)` for the school page, comparing against similar schools average, LA and England
- `Measure.ForSchoolComparison(...)` for the comparison page, comparing two schools side by side

SubMeasure types:

| Type                         | Content                                           | Included in                                |
| ---------------------------- | ------------------------------------------------- | ------------------------------------------ |
| `ThreeYearAverageSubMeasure` | `IEnumerable<decimal?>`, one value per comparator | Both `ForSchool` and `ForSchoolComparison` |
| `TopPerformersSubMeasure`    | Top 3 schools by three-year average               | `ForSchool` only                           |
| `YearByYearSubMeasure`       | Current, Previous and Previous2 series per comparator | Both                                    |

`SchoolData` is the input record that bundles everything a measure needs:

```
internal sealed record SchoolData(
    string Urn,
    string Name,
    Ks4PerformanceData? PerformanceData,
    Ks4DestinationsData? DestinationsData);
```

For primary, `Ks2PerformanceData` is used through `MeasureFieldSelector<Ks2PerformanceData>` in the same way.

### `MeasureViewModel`

Wraps a `Measure` for the view layer. Used by both phases.

```
MeasureViewModel.FromMeasure(measure, schoolDetails, labels[])
```

Labels come from the controller, for example `["School name", "Similar schools average", "Local authority schools average", "Schools in England average"]`.

### View components (partials)

| Partial                         | Purpose                                                      |
| ------------------------------- | ------------------------------------------------------------ |
| `_Measure`                      | Renders a complete measure with all its sub-measures as tabs |
| `_MeasureFilters`               | Renders filter dropdowns for a measure dynamically           |
| `_MeasureThreeYearAverageChart` | Bar chart for the three-year average sub-measure             |
| `_MeasureTopPerformers`         | Top performers panel                                         |
| `_MeasureYearByYearChart`       | Year-by-year line chart                                      |
| `_MeasureTable`                 | Data table view                                              |

### `<tabbed-view>` tag helper

`TabbedViewTagHelper` and `TabbedContentTagHelper` produce GOV.UK tabs markup from Razor without repeating the tab list and panel structure:

```
<tabbed-view html-prefix="attainment8">
  <tab-content id="chart" name="Three year average">...</tab-content>
  <tab-content id="top-performers" name="Top performers">...</tab-content>
  <tab-content id="year-by-year" name="Year by year">...</tab-content>
</tabbed-view>
```

### JavaScript modules

| Module                     | Purpose                                                                                                                       |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `chart-factory.js`         | ES module with `init(element)` and `initAll()`. `init` scopes chart setup to a DOM subtree for partial refreshes              |
| `measure-filters.js`       | Intercepts filter `<select>` changes, fetches the updated partial over AJAX, swaps the measure section, then re-initialises charts and tabs |
| `mobile-collapsed-tabs.js` | Extends GOV.UK `Tabs`. Always renders collapsed on mobile, and adds `selectTabById()` to restore tab state after a partial refresh |

---

## 10. Use cases and business rules

All use cases implement `IUseCase<TRequest, TResponse>` with `Execute(TRequest)` returning `Task<TResponse>`.

`GetSchoolInfoUseCase` is used on every page in both phases and fetches school info from `v_establishment`.

### Primary use cases (`SAPSec.Core/Features/Primary/`)

`GetSchoolKs2PerformanceMeasuresUseCase`

- fetches the similar schools group, then KS2 performance for all those schools
- applies `FilterBy` case-insensitively, for example the subject filter
- returns 6 `Measure` objects through `Ks2PerformanceMeasures.*.ForSchool()`

`GetSchoolKs2PerformanceComparisonUseCase`

- fetches only the two schools, with no group lookup
- returns 6 `Measure` objects through `Ks2PerformanceMeasures.*.ForSchoolComparison()`

`GetSchoolAttendanceMeasuresUseCase` returns one `Measure`, either overall or persistent absence.

`FindPrimarySimilarSchoolsUseCase`

- loads the group and its characteristic values
- applies `SimilarSchoolsFilters`: location, region, urban or rural, school type, admissions, gender, nursery, resourced provision, sixth form, attendance and school characteristics
- validates filters, returning `ValidationErrors` but still rendering the page rather than an error
- sorts by the chosen KS2 metric, default `RwmExpected`, tie-breaking on display value then alphabetically
- paginates to `ResultsPerPage`, default 10, and returns the full result set separately for the map

Sort options: `RwmExpected` (default), `RwmHigher`, `ReadingScaledScore`, `MathsScaledScore`, `GpsExpected`, `GpsHigher`.

`GetPrimarySimilarSchoolDetailsUseCase` coordinates both schools and pulls GIAS detail for the similar school.

### Secondary use cases (`SAPSec.Core/Features/Secondary/` and `SAPSec.Core/Features/`)

`GetSchoolKs4HeadlineMeasures` returns KS4 performance covering Attainment 8, English and Maths, and Destinations, as `Measure` objects.

`GetSchoolKs4CoreSubjects` returns 7 subjects (English Language, English Literature, Biology, Chemistry, Physics, Maths, Combined Science) with a grade filter of 4, 5 or 7, as `IReadOnlyCollection<Measure>`.

`GetFilteredSchoolKs4CoreSubject` serves the legacy `/data` JSON endpoints.

`FindSimilarSchools` handles secondary similar schools. It does not implement `IUseCase<T,R>` and uses inline LINQ rather than data providers. It will be refactored to match `FindPrimarySimilarSchoolsUseCase`.

`GetSchoolComparisonKs4HeadlineMeasures` serves the comparison page and returns a `Measure`-based response.

`GetSchoolComparisonKs4CoreSubjects` serves the comparison page and returns `IReadOnlyCollection<Measure>`.

`GetAttendanceMeasures` is shared across both phases and returns the attendance series and top performers.

---

## 11. Performance measures domain model

### Primary, KS2 (`SAPSec.Core/Features/Primary/Ks2PerformanceMeasures.cs`)

Six static inner classes, each with `ForSchool()` and `ForSchoolComparison()`. They use `MeasureFieldSelector<Ks2PerformanceData>` selecting Current, Previous and Previous2 across Establishment, LA and England.

| Measure                      | Data type         | Subject filter                       |
| ---------------------------- | ----------------- | ------------------------------------ |
| `MeetingExpectedStandardRwm` | `GradePercentage` | Yes, Reading, Writing, Maths, Combined |
| `AchievedHigherStandardRwm`  | `GradePercentage` | Yes, Reading, Writing, Maths, Combined |
| `AverageScaledScoreReading`  | `ScaledScore`     | No                                   |
| `AverageScaledScoreMaths`    | `ScaledScore`     | No                                   |
| `MeetingExpectedStandardGps` | `GradePercentage` | No                                   |
| `AchievedHigherStandardGps`  | `GradePercentage` | No                                   |

### Secondary, KS4 (`SAPSec.Core/Features/Measures/`)

KS4 measures use the same `Measure.ForSchool()` and `ForSchoolComparison()` factories, through `SAPSec.Core/Features/Measures/Ks4HeadlineMeasures.cs` and `Ks4CoreSubjects.cs`.

| Namespace             | Measures                                                                                                                                  |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `Ks4HeadlineMeasures` | `Attainment8`, `EnglishAndMaths` (grade 4 or 5 filter), `Destinations` (all, education or employment filter)                              |
| `Ks4CoreSubjects`     | `EnglishLanguage`, `EnglishLiterature`, `Biology`, `Chemistry`, `Physics`, `Mathematics`, `CombinedScience`, all with a grade 4, 5 or 7 filter |

`MeasureHelper`, which replaces `Ks4HeadlineMeasuresCalculator`, provides the shared `AverageFrom()`, `SeriesFrom()` and `ParseNullableDecimal()`.

---

## 12. Repositories and data sources

### Phase-specific

| Repository                                  | Interface                            | Views or source                                                                                 | Phase     |
| ------------------------------------------- | ------------------------------------ | ----------------------------------------------------------------------------------------------- | --------- |
| `PostgresSimilarSchoolsPrimaryRepository`   | `ISimilarSchoolsPrimaryRepository`   | `v_similar_schools_primary_groups`, `v_similar_schools_primary_values`                          | Primary   |
| `PostgresSimilarSchoolsSecondaryRepository` | `ISimilarSchoolsSecondaryRepository` | `v_similar_schools_secondary_groups`, `v_similar_schools_secondary_values`                      | Secondary |
| `JsonKs2PerformanceRepository`              | `IKs2PerformanceRepository`          | JSON files: `establishment_performance.json`, `la_performance.json`, `england_performance.json` | Primary, interim |
| `PostgresKs4PerformanceRepository`          | `IKs4PerformanceRepository`          | `v_establishment_performance`, `v_la_performance`, `v_england_performance`                      | Secondary |
| `PostgresKs4DestinationsRepository`         | `IKs4DestinationsRepository`         | `v_establishment_destinations`, `v_la_destinations`, `v_england_destinations`                   | Secondary |

KS2 performance is the one place that does not read from PostgreSQL. It is served from JSON files for now, and will move onto database views in the same way as KS4. See section 15.

### Shared across both phases

`PostgresEstablishmentRepository` reading `v_establishment`, `PostgresAbsenceRepository`, `ISchoolDetailsService` and `PostgresEstablishmentEmailRepository`.

### How the DTOs are generated

The DTOs are not written by hand. The chain is:

1. the `SAPData` project generates the SQL scripts
2. running `run-all.sql` against a local PostgreSQL database builds the views and writes out a JSON file per view
3. `SAPSec.DtoGenerator` reads those JSON files and generates the C# DTOs

The JSON files are produced anyway, because the automated tests use them as fixtures. Generating the DTOs from files that already exist is simpler than connecting to the database and reading catalogue metadata to do the same thing.

This replaced an earlier approach where the JSON files and the DTOs were kept in step with the views by hand, which was error-prone.

Two things to know when working on this:

- **The generator has to be run at the right time.** If a view changes and nobody reruns `SAPSec.DtoGenerator`, the DTOs drift from the database and nothing warns you. This is the main weakness of the approach. It is comparable to remembering to regenerate EF models, except EF can warn when its models no longer match the local database.
- **These JSON files are not a runtime data source.** They exist for DTO generation and tests. The exception is KS2 performance, covered in section 15.

---

## 13. Error handling

`NotFoundException` thrown by any use case is caught by `NotFoundExceptionHandler`, registered through `services.AddExceptionHandler<NotFoundExceptionHandler>()`:

- logs a warning
- sets HTTP 404 and rewrites the path to `/error/404`
- outside production, surfaces `ex.Message` for debugging

`ErrorController` sits at `[Route("error")]` with `[AllowAnonymous]`. 401 and 403 render `AccessDenied.cshtml`, 404 renders `NotFound.cshtml`, and anything else renders `Problem.cshtml`.

Middleware pipeline order, relevant excerpt:

```
UseStatusCodePagesWithReExecute("/error/{0}")
UseDeveloperExceptionPage | UseExceptionHandler  ← triggers NotFoundExceptionHandler
UseMiddleware<SecurityHeadersMiddleware>
UseAuthentication / UseAuthorization
MapControllers
```

---

## 14. School layout and side navigation

Both phases call a `SetSchoolViewData()` or `PopulateViewData()` helper on every action. It sets `ViewData["SchoolNavigation"]` using phase-specific factories:

- `SchoolSideNavigationViewModel.CreatePrimary(Url, urn, actionName)` gives Overview, KS2, Attendance, View similar schools, School details
- `SchoolSideNavigationViewModel.CreateSecondary(Url, urn, actionName)` gives Overview, KS4 Headline Measures, KS4 Core Subjects, Attendance, View similar schools, School details

Comparison layouts in both phases read `ViewData["ComparisonSchool"]` to render the comparison header and sub-navigation.

---

## 15. Data pipeline dependency

Most data is read live from PostgreSQL on each request. KS2 performance is the exception.

### Packaged into the deployment artefact

The KS2 JSON files (`establishment_performance.json`, `la_performance.json`, `england_performance.json`) are generated by SAPData and shipped with the build. Updated KS2 data therefore needs a redeploy to take effect.

This is deliberate for now rather than a permanent design. KS2 will move onto PostgreSQL views in the same way as KS4, at which point the redeploy dependency goes away.

### Read live from PostgreSQL

- `v_similar_schools_primary_groups` and `v_similar_schools_primary_values`, primary similar schools
- `v_similar_schools_secondary_groups` and `v_similar_schools_secondary_values`, secondary similar schools
- `v_establishment_performance`, `v_la_performance`, `v_england_performance`, KS4 performance
- `v_establishment_destinations`, `v_la_destinations`, `v_england_destinations`, KS4 destinations
- `v_establishment`, all school info

---

## 16. Test coverage

| Project                                 | Scope                                                                                                                                                                                                    |
| --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Tests/SAPSec.Core.Tests`               | Unit. All KS2 measures (6 measures, all filter variations), `FindPrimarySimilarSchoolsUseCase` (all sort keys, tie-breaking, pagination, filter validation, `NotFoundException`), KS4 headline and core subject use cases, `EstablishmentExtensions.CanSearch`, `SchoolSearchService` phase and flag behaviour |
| `Tests/SAPSec.Web.Tests`                | Unit. Controllers (primary redirect, feature flag off), `NotFoundExceptionHandler` (404 with warning log, 500 with error log)                                                                            |
| `Tests/SAPSec.Infrastructure.Tests`     | Unit. Lucene abbreviation expansion (`St` to `Saint`), multi-token search, prefix matching                                                                                                               |
| `Tests/SAPSec.Test.InMemoryIntegration` | Integration with in-memory repositories and no database. All primary and secondary routes return 200, feature flag off gives 404 for primary, KS2 and KS4 measures with correct table data and filter behaviour, similar schools filter, sort and pagination, comparison measures and accessibility assertions |
| `Tests/SAPSec.Test.EndToEnd`            | Playwright. Full user journeys, search through school detail through comparison, for both phases                                                                                                         |
| `Tests/SAPSec.Test.Accessibility`       | Playwright with axe-core, WCAG 2.1 AA. All pages in both phases including all comparison sub-pages                                                                                                       |

`SAPSec.Test.InMemoryIntegration` was previously named `SAPSec.Test.Integration`. It uses in-memory repository doubles such as `InMemoryKs2PerformanceRepository`, `InMemorySimilarSchoolsPrimaryRepository` and `InMemoryKs4PerformanceRepository`, and needs no database.

Test builders live in `Tests/SAPSec.Test.Common`:

- `Build.Establishment("urn", "name", x => x.Primary().Open().InLA("001"))` and `.Secondary()`
- `Build.PrimaryGroup(urn, neighbourUrns[])` and `Build.SecondaryGroup(...)`
- `Build.Ks2Performance.Establishment(urn, x => x.WithRwmExpected(current, prev, prev2))`

---

## 17. Phase comparison and refactoring notes

| Aspect                                        | Primary                                                                             | Secondary                                                                                                     |
| --------------------------------------------- | ----------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Performance data source                       | JSON files through `JsonKs2PerformanceRepository`, interim                          | PostgreSQL through `PostgresKs4PerformanceRepository`                                                          |
| Performance data type                         | `Ks2PerformanceData`                                                                | `Ks4PerformanceData`                                                                                           |
| Measure domain classes                        | `Ks2PerformanceMeasures`, 6 measures                                                | `Ks4HeadlineMeasures`, `Ks4CoreSubjects`                                                                       |
| `Measure` and `SubMeasure` pattern            | Fully implemented                                                                   | In place on school pages, comparison pages still being brought across                                          |
| View components (`_Measure`, `<tabbed-view>`) | Fully implemented                                                                   | Being adopted                                                                                                  |
| JSON `/data` endpoints                        | Not used                                                                            | Still routed, legacy, being removed                                                                            |
| Similar schools repository                    | `ISimilarSchoolsPrimaryRepository`                                                  | `ISimilarSchoolsSecondaryRepository`                                                                           |
| Find similar schools use case                 | `FindPrimarySimilarSchoolsUseCase`, implements `IUseCase<T,R>`, uses data providers | `FindSimilarSchools`, does not implement `IUseCase`, inline LINQ, will be refactored to match primary          |
| Feature flag                                  | `EnablePrimarySchools` gates all routes                                             | No equivalent flag                                                                                             |
| Side nav                                      | Overview, KS2, Attendance, Similar schools, School details                          | Overview, KS4 Headline, KS4 Core Subjects, Attendance, Similar schools, School details                         |

Shared across both phases: `GetSchoolInfoUseCase`, `GetAttendanceMeasures`, `IEstablishmentRepository`, `IAbsenceRepository`, `SimilarSchoolsFilters`, `SchoolSearchController` and `SchoolSearchService`, `Measure`, `SubMeasure` and `MeasureViewModel`, `ISimilarSchoolsPageViewModel`, `ISimilarSchoolRowViewModel`, `NotFoundExceptionHandler`, DSI authentication and `SecurityHeadersMiddleware`.

---

## 18. Class diagrams

### 18.1 Search pipeline and phase-aware filtering

```mermaid
classDiagram
    class ISchoolSearchService {
        <<interface>>
        +SearchAsync(query) Task~IReadOnlyList~SchoolSearchResult~~
        +SearchByNumberAsync(number) Task~Establishment~
        +SuggestAsync(queryPart) Task~IReadOnlyList~SchoolSearchResult~~
    }

    class SchoolSearchService {
        -ISchoolSearchIndexReader _indexReader
        -IEstablishmentRepository _establishmentRepository
        -IFeatureFlagService _featureFlagService
        +SearchAsync(query)
        +SearchByNumberAsync(number)
        +SuggestAsync(queryPart)
        -SearchInternalAsync(query, maxResults, includeCoordinates)
    }

    class ISchoolSearchIndexReader {
        <<interface>>
        +SearchAsync(query, maxResults) Task~IList~(int urn, string resultText)~~
    }

    class LuceneShoolSearchIndexReader {
        -LuceneIndexContext context
        -LuceneTokeniser tokeniser
        -LuceneHighlighter highlighter
        +SearchAsync(query, maxResults)
    }

    class LuceneIndexContext {
        +Directory RAMDirectory
        +Analyzer LuceneTokenAnalyser
        +Writer IndexWriter
        +SearcherManager SearcherManager
    }

    class EstablishmentExtensions {
        <<static>>
        +CanIndexForSearch(establishment) bool
        +CanSearch(establishment, primaryEnabled) bool
    }

    class PhaseOfEducationValues {
        <<static>>
        +PrimaryId = "2"
        +AllThroughId = "7"
        +SecondaryId = "4"
        +IsPrimaryOrAllThrough(phase) bool
        +IsSearchableSearchPhaseId(phaseId, primaryEnabled) bool
    }

    class SchoolSearchController {
        +Index() IActionResult
        +Search(query, localAuthorities, page)
        +Suggest(queryPart)
        -BuildSchoolUrl(urn, phase) string
    }

    SchoolSearchService ..|> ISchoolSearchService
    SchoolSearchService --> ISchoolSearchIndexReader
    SchoolSearchService --> IFeatureFlagService
    SchoolSearchService --> EstablishmentExtensions : CanSearch()
    LuceneShoolSearchIndexReader ..|> ISchoolSearchIndexReader
    LuceneShoolSearchIndexReader --> LuceneIndexContext
    EstablishmentExtensions --> PhaseOfEducationValues
    SchoolSearchController --> ISchoolSearchService
    SchoolSearchController --> PhaseOfEducationValues : IsPrimaryOrAllThrough()
```

---

### 18.2 Measure domain model, both phases

```mermaid
classDiagram
    class Measure {
        +Key string
        +Name string
        +DataType MeasureDataType
        +Filters IEnumerable~MeasureAvailableFilter~
        +SubMeasures IEnumerable~SubMeasure~
        +ForSchool(...)$
        +ForSchoolComparison(...)$
    }

    class SubMeasure {
        <<abstract>>
    }

    class ThreeYearAverageSubMeasure {
        +Averages IEnumerable~decimal~~
        +ForSchool(schoolData, similarSchools, selector)$
        +ForSchoolComparison(current, similar, selector)$
    }

    class TopPerformersSubMeasure {
        +TopPerformers IEnumerable~TopPerformer~
        +ForSchool(schoolData, similarSchools, selector)$
    }

    class YearByYearSubMeasure {
        +Series IEnumerable~YearByYearSeries~
        +ForSchool(schoolData, similarSchools, selector)$
        +ForSchoolComparison(current, similar, selector)$
    }

    class Ks2PerformanceMeasures {
        <<static, primary>>
        +MeetingExpectedStandardRwm ForSchool() / ForSchoolComparison()
        +AchievedHigherStandardRwm ForSchool() / ForSchoolComparison()
        +AverageScaledScoreReading ForSchool() / ForSchoolComparison()
        +AverageScaledScoreMaths ForSchool() / ForSchoolComparison()
        +MeetingExpectedStandardGps ForSchool() / ForSchoolComparison()
        +AchievedHigherStandardGps ForSchool() / ForSchoolComparison()
    }

    class Ks4HeadlineMeasures {
        <<static, secondary>>
        +Attainment8 ForSchool() / ForSchoolComparison()
        +EnglishAndMaths ForSchool() / ForSchoolComparison()
        +Destinations ForSchool() / ForSchoolComparison()
    }

    class Ks4CoreSubjects {
        <<static, secondary>>
        +EnglishLanguage ForSchool() / ForSchoolComparison()
        +EnglishLiterature ForSchool() / ForSchoolComparison()
        +Biology / Chemistry / Physics / Mathematics / CombinedScience
    }

    class MeasureViewModel {
        +Key string
        +Name string
        +SubMeasureViewModels IEnumerable~SubMeasureViewModel~
        +AvailableFilters IEnumerable~MeasureAvailableFilterViewModel~
        +FromMeasure(measure, schoolDetails, labels[])$
    }

    class IUseCase~TReq_TResp~ {
        <<interface>>
        +Execute(request) Task~TResp~
    }

    class GetSchoolKs2PerformanceMeasuresUseCase {
        +Execute() 6x Measure
    }

    class GetSchoolKs4HeadlineMeasures {
        +Execute() 3x Measure
    }

    SubMeasure <|-- ThreeYearAverageSubMeasure
    SubMeasure <|-- TopPerformersSubMeasure
    SubMeasure <|-- YearByYearSubMeasure
    Measure "1" *-- "many" SubMeasure

    Ks2PerformanceMeasures ..> Measure : creates
    Ks4HeadlineMeasures ..> Measure : creates
    Ks4CoreSubjects ..> Measure : creates

    MeasureViewModel ..> Measure : wraps

    GetSchoolKs2PerformanceMeasuresUseCase ..|> IUseCase
    GetSchoolKs2PerformanceMeasuresUseCase ..> Ks2PerformanceMeasures
    GetSchoolKs2PerformanceMeasuresUseCase ..> Measure : returns

    GetSchoolKs4HeadlineMeasures ..|> IUseCase
    GetSchoolKs4HeadlineMeasures ..> Ks4HeadlineMeasures
    GetSchoolKs4HeadlineMeasures ..> Measure : returns
```
