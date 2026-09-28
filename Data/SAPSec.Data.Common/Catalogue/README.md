# Data map catalogue

The data map says, for every property the website shows, which CSV file it comes from, which column holds the value
and which rows to read. It is defined in code, one file per dataset, in [Definitions](Definitions):

| Dataset | File |
|---|---|
| KS4 performance | [Ks4Performance.cs](Definitions/Ks4Performance.cs) |
| KS4 destinations | [Ks4Destinations.cs](Definitions/Ks4Destinations.cs) |
| KS2 performance | [Ks2Performance.cs](Definitions/Ks2Performance.cs) |
| Pupil absence | [PupilAbsence.cs](Definitions/PupilAbsence.cs) |
| Workforce | [Workforce.cs](Definitions/Workforce.cs) |
| School email | [SchoolEmail.cs](Definitions/SchoolEmail.cs) |
| Similar schools | [SimilarSchools.cs](Definitions/SimilarSchools.cs) |

Every dataset is listed in [CatalogueDefinitions.cs](Definitions/CatalogueDefinitions.cs). Establishment details
(`v_establishment`) are not in the catalogue; they are read from the daily GIAS file in
`SAPData/GenerateViews.cs` (`GenerateEstablishmentDimensionView`).

## How it fits together

```
Definitions/*.cs            what to read: files, years, pupil groups, measures and filters
   │  MeasureSet / ColumnSet        (both implement IDataMapDefinition: "turn me into rows")
   ▼
DataMapRow list             one row per property: file, key column, value column, filters
   │
   ├─► CatalogueValidator       stops the run on a mistake (see Validation below)
   ├─► SourceFileCheck          stops the run if the downloaded files don't match
   └─► GenerateViews            one SQL view per dataset and scope, listed in GenerateViews.Views
```

Each part has one job, and adding things doesn't mean changing the machinery:

| To add | Change |
|---|---|
| a measure | a `.Metric(...)` line in the dataset's definition |
| a pupil group in a file | a `.Provides(...)` line on that file's `Source` |
| a new year | `DataYears.cs` and the file names ([runbook](../../../docs/operational/003-new-data-year.md)) |
| a dataset | a new file in `Definitions`, listed in `CatalogueDefinitions` |
| a view | one line in `GenerateViews.Views` |

## How a value is found

Each property reads one cell: **one file**, **one row per school, LA or England** and **one column**.

```
file        202425_performance_tables_schools_final.csv
key column  school_urn               → one row per school
value       attainment8_average      → the column holding the number
rows where  breakdown = 'Boys'       → filters that pick the right row
        and time_period = '202425'
```

## Where filters are set

A CSV usually has several rows per school, for example one per pupil group or year. Filters pick the one row a
property needs. They are set in three places, from the most general to the most specific:

| Where | Applies to | Used for | Example |
|---|---|---|---|
| `Source.Where(column, value)` | every property read from that file | year, geography, "Total" rows | `.Where("time_period", year.Code)` |
| `Source.Provides(breakdown, (column, value))` | one pupil group from that file | Total, Boys, Girls, Disadvantaged, … | `.Provides(Breakdowns.Boys, ("breakdown", "Boys"))` |
| `Metric.Where(column, value)` | one measure | subject, grade, destination | `.Where("subject", "Biology")` |

`Metric.Where(source, column, value)` is the same as `Metric.Where` but only for one file, for when the files name
things differently (for example subject names in newer files, discount codes in older ones).

A property's filters are all three combined, so every filter must match for a row to be read. A filter with several
values (`.Where("grade", "7", "8")`) matches any of them.

### Example: Attainment 8 for boys, schools, 2024-25

In [Ks4Performance.cs](Definitions/Ks4Performance.cs):

```csharp
// The file, its key column and the rows for the year: Source.Where
private static Source PerformanceTables(string file, AcademicYear year, string eal, string notDisadvantaged) =>
    Source.Ees(file)
        .KeyedBy("school_urn")
        .Where("time_period", year.Code)
        // One pupil group each: Source.Provides
        .Provides(Breakdowns.Total, ("breakdown", "Total"))
        .Provides(Breakdowns.Boys, ("breakdown", "Boys"))
        ...

// The measure and the column holding its value
.Metric(new Metric("Attainment8", "attainment8_average")
    .Field(schoolsPrevious, "avg_att8"))      // the 2023-24 file names the column differently
```

## Checking a property

To see exactly where one property comes from, without reading the definitions:

```
dotnet run --project SAPData -- explain Attainment8_Boy_Est_Current_Num
```

```
Attainment8_Boy_Est_Current_Num
  Dataset:     KS4_Performance (Performance), Establishment
  Year:        Current 2024-2025
  File:        202425_performance_tables_schools_final (EES)
  Key column:  school_urn
  Value from:  attainment8_average
  Rows where:  breakdown = 'Boys'
          and  time_period = '202425'
```

Give part of a name (for example `explain Attainment8_Boy`) to list matching properties. To list each dataset's
years and files, run `dotnet run --project SAPData -- catalogue-summary`.

## Validation

The data map is checked before any SQL is generated. Each rule is its own class in [Validation/Rules](Validation/Rules),
implementing [IValidationRule](Validation/IValidationRule.cs), and [CatalogueValidator.cs](Validation/CatalogueValidator.cs)
lists the ones that run. To add a rule, write a class and add it to `CatalogueValidator.Rules`; to change one, edit only
its class. Each is based on a real error found in the old spreadsheet:

| Rule | Checks that |
|---|---|
| `period-in-name` ([NameMatchesYear](Validation/Rules/NameMatchesYear.cs)) | a property named `…_Previous2_…` reads the Previous2 year |
| `time-period-filter` ([TimePeriodMatchesYear](Validation/Rules/TimePeriodMatchesYear.cs)) | a `time_period` filter is for the property's year |
| `duplicate-mapping` ([NoDuplicateMappings](Validation/Rules/NoDuplicateMappings.cs)) | no two properties read the same file, column and filters |
| `key-column` ([OneKeyColumnPerFile](Validation/Rules/OneKeyColumnPerFile.cs)) | every property reading a file in one view uses the same key column |
| `unknown-file` ([FileExists](Validation/Rules/FileExists.cs)) | the file exists |
| `unknown-column` ([ColumnsExist](Validation/Rules/ColumnsExist.cs)) | the value column, key column and every filter column are in the file |
| `unknown-value` ([FilterValuesExist](Validation/Rules/FilterValuesExist.cs)) | every filter value occurs in the file |

The last three compare the data map with the real files:

- in unit tests, against [source-profiles.json](../../../SAPData/DataMap/source-profiles.json), a committed
  snapshot of each file's columns and filter values;
- in every pipeline run, against the files just downloaded, before anything is loaded.

A renamed column or pupil group in a new DfE file is therefore reported, with the property, file and value, instead
of loading as blank figures. See [Adding a new year of data](../../../docs/operational/003-new-data-year.md).
