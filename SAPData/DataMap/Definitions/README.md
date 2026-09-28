# Data map definitions

These JSON files are the data map: for every figure the website shows, which source file it comes from, which row
(key column and filters) and which column holds the value. Developers and data engineers edit them the same way;
no C# is needed.

| Dataset | File |
|---|---|
| KS4 performance | [ks4-performance.json](ks4-performance.json) |
| KS4 destinations | [ks4-destinations.json](ks4-destinations.json) |
| Pupil absence | [pupil-absence.json](pupil-absence.json) |
| KS2 performance | [ks2-performance.json](ks2-performance.json) |
| School email | [school-email.json](school-email.json) |
| Workforce | [workforce.json](workforce.json) |
| Similar schools | [similar-schools.json](similar-schools.json) |

Every dataset is listed in [catalogue.json](catalogue.json). School details (`v_establishment`) are not in the data
map; they come from the daily GIAS file in `SAPData/GenerateViews.cs` (`GenerateEstablishmentDimensionView`).

[dataset.schema.json](dataset.schema.json) describes the format. VS Code and Visual Studio use it (through each
file's `"$schema"`) to suggest settings and underline mistakes as you type.

## How a value is found

Each property reads one cell: **one file**, **one row per school, LA or England** and **one column**.

```
file        202425_performance_tables_schools_final
key         school_urn               → one row per school
column      attainment8_average      → the column holding the number
rows where  breakdown = 'Boys'       → filters that pick the right row
        and time_period = '202425'
```

## The parts of a dataset file

```jsonc
{
  "dataset": "KS4_Performance",
  "measureSets": [{
    "subtype": "Performance",
    "yearsFrom": "Ks4Performance",            // the current year, from DataYears.cs (shared with the website)
    "pupilGroups": ["Total", "Boys", "Girls"], // groups each measure is broken down by
    "sources": [{                              // which file each scope and year reads
      "id": "schoolsCurrent",
      "scopes": ["Establishment"],
      "periods": ["Current"],
      "publisher": "EES",
      "file": "202425_performance_tables_schools_final",
      "key": "school_urn",
      "where": { "time_period": "{year}" },    // rows for every measure from this file
      "pupilGroups": {                         // the rows for each pupil group
        "Total": { "breakdown": "Total" },
        "Boys": { "breakdown": "Boys" }
      }
    }],
    "measures": [{                             // one entry per measure
      "name": "Attainment8",
      "column": "attainment8_average"
    }]
  }]
}
```

Each measure becomes one property per scope, year and pupil group, named `{name}_{group}_{scope}_{period}_{unit}`,
e.g. `Attainment8_Boy_Est_Current_Num`.

## Where filters are set

A source file usually has several rows per school, for example one per pupil group and year. Filters pick the one
row a property needs. They are set in three places, from the most general to the most specific, and combined:

| Where | Applies to | Example |
|---|---|---|
| a source's `where` | every measure read from that file | `"where": { "time_period": "{year}" }` |
| a source's `pupilGroups` | one pupil group from that file | `"Eal": { "breakdown": "Known or believed to be other than English" }` |
| a measure's `where` | one measure | `"where": { "subject": "Biology" }` |

A list of values means any of them: `"grade": ["7", "8"]`. `{year}` is the year code for the source's period, e.g.
`202425`.

When files name things differently, a measure can say what to use in each source (by the source's `id`):

- `"columnBySource": { "schoolsPrevious": "avg_att8" }`: a different column in that file
- `"columnBySource": { "cscp": { "Total": "ATT8SCR", "Boys": "ATT8SCR_BOYS" } }`: wide files with one column per
  pupil group
- `"whereBySource": { "older": { "Discount Code": "RH3" } }`: filters for that file only

Other settings: `"unit": "Pct"` for percentages; `"pupilGroups"`, `"scopes"` and `"periods"` on a measure to limit
it; `"skip"` for one scope and period a file doesn't publish; `"nameTemplate"` for other naming schemes
(placeholders `{metric}`, `{breakdown}`, `{_breakdown}`, `{scope}`, `{period}`, `{unit}`).

## Avoiding repetition

- **`pupilGroupSets`**: name a set of pupil group filters once and use it from several sources, e.g.
  `"pupilGroups": "characteristics"`. A list combines sets and extra groups:
  `"pupilGroups": ["destinations", { "Eal": { … } }]`.
- **One source, several scopes**: `"scopes": ["England", "LA"]` with a key per scope,
  `"key": { "England": "geographic_level", "LA": "old_la_code" }`. A `where` value per scope applies only to the
  scopes it names.
- **`forEach`**: repeat measures (or a whole measure set) for every combination of values from `lists`:

  ```json
  {
    "forEach": { "subject": "subjects", "band": "gradeBands" },
    "measures": [{
      "name": "{subject.code}{band.code}",
      "column": "number_achieving",
      "where": { "subject": "{subject.name}", "grade": "{band.grade}" }
    }]
  }
  ```

## Common changes

| To | Edit |
|---|---|
| Add a measure | add an entry to `measures` |
| Change a column or filter | the measure's `column`/`where`, or the source's `where`/`pupilGroups` |
| Handle a label DfE renamed | that year's source `pupilGroups` (each year's file can have its own labels) |
| Move to a new data year | `DataYears.cs` and the file names ([runbook](../../../docs/operational/003-new-data-year.md)) |
| Add a dataset | a new file here, listed in `catalogue.json` |

## After a change

1. Check it: `dotnet run --project SAPData -- explain <PropertyName>` shows the file, key, column and filters one
   property ends up with.
2. Regenerate the mapping list: `dotnet run --project SAPData -- export-map`. [datamap.generated.json](../datamap.generated.json)
   lists every property on one line, so the pull request shows exactly which figures changed.
3. Run the tests: `dotnet test Tests/SAPSec.Data.Common.Tests`. They check every definition against a snapshot of
   the real source files (see [validation](../../../Data/SAPSec.Data.Common/Catalogue/README.md#validation)) and fail
   if the mapping list is out of date. A mistake in a file is reported with the file and setting, e.g.
   `ks4-performance.json: $.measureSets[0].measures[0] (Attainment8): unknown setting(s) 'colum'`.
4. Raise a pull request with the definition and `datamap.generated.json`.

A new property appears in its database view straight away; the website shows it once a developer adds it to a page.
