# Data release runbook October 2026

## Local development
Assumption: this set of tasks is carried out by a single developer on their local development environment

### 1. Copy dummy data files to local dev environment

### 2. [Optional: Process data files locally via manual scripts to remove unneeded data]
Public service have developed a set of scripts to remove unneeded data rows from source data files to reduce 
data pipeline run time and risk of Konduit failing when importing files. 
It may be possible to copy these and run them but would need to be tested to make sure only unneeded data is removed.

### 3. Update data map with new files/columns
[*] Notes should be made of which columns have been added/updated to aid future testing

### 4. Run data pipeline against local DB

### 5. Regenerate DTOs and rebuild solution, resolve any build errors due to missing/updated properties
[*] Update notes as required

### 6. Test service locally, identify and resolve any data issues
[*] Update notes as required

### 7. Create Data Release PR and test in review app

### 8. Get rest of dev team to review and approve Data Release PR

## Pre-release (12th-14th October)
### 1. Edit workflows to skip enabling/disabling maintenance page on prod (identify + list)
For each step of each workflow identified, comment out current `if:` condition and add `if: false` condition

Workflows:
* Build and Deploy (build-and-deploy.yml)
  
  (check: this only deploys to maintenance service and doesn't actually enable/disable maintenance page)
* School Data Ingestion Pipeline (data-pipeline.yml)

  Jobs:
  * `ingest`
    
    Steps:
    * `Enable maintenance page`
    * `Summarise maintenance enable`
    * `Disable maintenance page`
    * `Summarise maintenance restore`

### 2. Enable maintenance page on prod
Run workflow: **Manage Website Maintenance Mode** (toggle-maintenance-page.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to update| `production`|
|Route traffic to the maintenance page or back to the app| `enable`|

### 3. Upload dummy data files from local dev environment to test blob storage
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdptssa`|
|Blob container|`schooldata`|

### 4. Run data pipeline against test DB
### 5. Ensure data release PR is up-to-date, tests passing, no merge conflicts
### 6. Resolve any issues in data release PR found in (4)
### 7. Merge data release PR

### 8. Test service on test environment, resolving any issues
Use notes created in local development for targeted testing

## Pre-release (15th October before 9:30)
### 1. Rename dummy data files in test blob storage
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdptssa`|
|Blob container|`schooldata`|

## Release (15th October from 9:30)
### 1. Upload released data files to test blob storage (ensuring same names as dummy data files)
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdptssa`|
|Blob container|`schooldata`|
### 2. Run data pipeline against test DB
### 3. Create backup of test DB (workflow, filename)
### 4. Download backup file from test blob storage (location)
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdbbkptssa`|
|Blob container|`database-backup`|
|File||
### 5. Upload backup file to prod (location)
### 6. Restore backup file against prod DB (workflow, filename)
### 7. Disable maintenance page on prod

## Post-release
1. Edit workflows to reinstate enabling/disabling maintenance page on prod
2. Delete dummy data files from test blob storage
3. Verify files in test blob storage by running data pipeline against test
4. Delete dummy data files in prod blob storage
5. Upload released data files to prod blob storage
6. Verify files in prod blob storage by visual inspection against test blob storage
