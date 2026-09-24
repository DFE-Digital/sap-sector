# Adding A New Year Of Data

## Purpose

How to move a dataset (KS4 performance, KS4 destinations, KS2 performance, pupil absence or workforce) on to a newly published academic year. Each dataset holds three years (Current, Previous, Previous2), so a new year drops the oldest.

The data map is code (`Data/SAPSec.Data.Common/Catalogue/Definitions`), so a new year is a small, reviewable change. Automated checks stop the change if the new files don't contain what the catalogue expects.

---

## What the year controls

`Data/SAPSec.Data/DataYears.cs` holds the current year for each dataset, as a start year (`2024` = 2024 to 2025). It is used by:

- **the pipeline**: which `time_period` each year's data is filtered to, and the file names that include a year
- **the website**: the year labels on every measure's chart and table

Because both read the same constant, the website labels always match the data that was loaded.

---

## Steps

### 1. Get the new files

- **EES API datasets** (listed in `SAPData/raw_sources.json` with a `DataSetId`) are downloaded by the pipeline when a new version is published. For local work, download the CSV from the EES data set page.
- **Other files** are uploaded once to blob storage with a `manual_` prefix, e.g. `manual_oa_percent_3term_sch_202526_ccss.csv`.

Put the files in `SAPData/DataMap/SourceFiles` (not committed).

### 2. Bump the year

In `Data/SAPSec.Data/DataYears.cs`, add one to the dataset's year, e.g. `PupilAbsence = 2025`.

### 3. Point the catalogue at the new files

Open the dataset's definition (e.g. `PupilAbsence.cs`). Most files are one of three kinds:

- **built from the year**, e.g. `$"{suffix}_percent_3term_sch_{year.Code}_ccss"`: nothing to change
- **one file with every year**, e.g. `6_absence_3term_characteristics`: nothing to change if DfE updates the same file; otherwise change the file name
- **a file per year with an irregular name**, e.g. `202425_performance_tables_schools_final`: add the new year's file and move the older ones down (Current → Previous → Previous2)

To see every file the catalogue now needs, and which are missing locally:

```
dotnet run --project SAPData -- catalogue-summary
```

Missing files are marked with `!`.

### 4. Refresh the source profiles

```
dotnet run --project SAPData -- profile-sources
```

This updates `SAPData/DataMap/source-profiles.json`: each source file's columns and the values of the columns the catalogue filters on.

### 5. Run the tests

```
dotnet test Tests/SAPSec.Data.Common.Tests
```

`Every_definition_is_valid_against_its_source_files` reports anything the new files don't support, for example:

- `filter time_period = '202526' matches no rows in 6_absence_3term_characteristics`: the file doesn't have the new year yet
- `filter breakdown = 'Other than English' matches no rows in …`: DfE renamed a breakdown (this happened between 2023-24 and 2024-25; give each year its own label)
- `field 'avg_att8' is not a column in …`: a column was renamed

Fix the definition, not the test.

The pipeline runs the same check against the files it downloads, before loading anything. If a new file arrives
before the catalogue has been updated for it (for example an automatically published EES version with renamed
columns), the run stops with the same messages and the website keeps the previous data until the catalogue is fixed.

### 6. Check the output locally (recommended)

Run the generator and load the views as described in `SAPData/README.md`, then spot-check a few schools on the website against the published figures.

### 7. Pipeline loading

Nothing to do: the pipeline loads any source file it hasn't loaded before (or that has changed), and rebuilds the views whose SQL changed. See "Incremental loads" in `SAPData/README.md`.

### 8. Raise the pull request

Include:

- `DataYears.cs`
- the catalogue definition
- `source-profiles.json`

---

## Worked example: dry run of pupil absence 2025-26

With `PupilAbsence = 2025` and no new files, `catalogue-summary` shows:

```
PupilAbsence
  Current 2025-2026
      6_absence_3term_characteristics
    ! oa_percent_3term_sch_202526_ccss
    ! pa_percent_3term_sch_202526_ccss
  Previous 2024-2025
      ...
```

and validation reports 80 issues, e.g.:

```
[unknown-value] PupilAbsence/England.Abs_Tot_Primary_Eng_Current_Pct: filter time_period = '202526' matches no rows in 6_absence_3term_characteristics
[unknown-file] PupilAbsence/Establishment.Abs_Tot_Est_Current_Pct: no profile for source file 'oa_percent_3term_sch_202526_ccss'; regenerate the source profiles
```

Adding the two school files and the updated characteristics file, then running `profile-sources`, clears both.
