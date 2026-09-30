# Data map catalogue

The data map is defined in JSON in [SAPData/DataMap/Definitions](../../../SAPData/DataMap/Definitions): start with
[its README](../../../SAPData/DataMap/Definitions/README.md). This folder is the engine that reads those files,
checks them and turns them into the rows the pipeline reads.

## How it fits together

```
SAPData/DataMap/Definitions/*.json    what to read: files, years, pupil groups, measures and filters
   │  Definitions/JsonDefinitions.cs      reads the files (built into this assembly)
   │  MeasureSet / ColumnSet              expand them (both implement IDataMapDefinition: "turn me into rows")
   ▼
DataMapRow list             one row per property: file, key column, value column, filters
   │
   ├─► CatalogueValidator       stops the run on a mistake (see Validation below)
   ├─► SourceFileCheck          stops the run if the downloaded files don't match
   ├─► GenerateViews            one SQL view per dataset and scope, listed in GenerateViews.Views
   └─► DataMapExport            the mapping list, SAPData/DataMap/datamap.generated.json
```

| Part | Job |
|---|---|
| [JsonDefinitions.cs](Definitions/JsonDefinitions.cs) | reads the JSON, expands `forEach`, reports mistakes with the file and setting |
| [MeasureSet.cs](MeasureSet.cs), [Metric.cs](Metric.cs), [Source.cs](Source.cs) | expand measures across scopes, years and pupil groups, combining filters |
| [ColumnSet.cs](ColumnSet.cs) | columns copied as they are (email, similar schools) |
| [Validation](Validation) | the rules below |
| [DataMapExport.cs](DataMapExport.cs) | the one-line-per-property mapping list |

Adding a measure, pupil group, dataset or year is a change to the JSON only. Adding a kind of setting the JSON can't
express yet is a change here: add it to `JsonDefinitions` (and its list of allowed settings), to
`dataset.schema.json`, and a test to `JsonDefinitionsTests`.

## The mapping list

[SAPData/DataMap/datamap.generated.json](../../../SAPData/DataMap/datamap.generated.json) lists every property, one per
line, generated from the definitions (`dotnet run --project SAPData -- export-map`):

```json
{"property":"Attainment8_EAL_Est_Current_Num","dataset":"KS4_Performance","subtype":"Performance","scope":"Establishment","period":"Current","year":"2024-2025","publisher":"EES","file":"202425_performance_tables_schools_final","keyColumn":"school_urn","valueColumn":"attainment8_average","dataType":"double","filters":{"breakdown":["Known or believed to be other than English"],"time_period":["202425"]}}
```

It is for looking up and reviewing mappings; edit the definitions, not the list. A test fails if the committed list
is out of date. Keep its format as generated (one mapping per line).

## Checking a property

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
