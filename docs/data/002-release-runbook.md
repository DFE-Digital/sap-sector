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
Run SAPData project locally to generate SQL script files, and execute `run-all.sql` from PGSQL

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
* **Build and Deploy** (build-and-deploy.yml)
  
  This only deploys to maintenance service and doesn't actually enable/disable maintenance page (TODO: check/confirm)
  
* **School Data Ingestion Pipeline** (data-pipeline.yml)

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
Run workflow: **School Data Ingestion Pipeline** (data-pipeline.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to run against| `test`|
|Pull request number of the review app| leave blank |
|Ignore the rebuild list and rebuild all raw tables| checked |
|Optional override path to the raw tables rebuild list|leave blank|

### 3. Create backup of test DB
Run workflow: **Backup database to Azure storage** (backup-db.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to backup| `test`|
|Backup file name| leave as default or enter file name |

### 4. Download backup file from test blob storage (location)
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdbbkptssa`|
|Blob container|`database-backup`|
|File|`sapsec_test_adhoc_2026-10-15` - adjust for backup date, or use filename entered in (3) |

### 5. Upload backup file to prod (location)
|Details||
|-|-|
|Environment|Production|
|Storage account|`s189p01sapsecdbbkppdsa`|
|Blob container|`database-backup`|
|File|`sapsec_test_adhoc_2026-10-15` - adjust for backup date, or use filename entered in (3) |

### 6. Restore backup file against prod DB
Run workflow: **Restore database from Azure storage** (postgres-restore.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to restore| `test`
|Must be set to true if restoring production| 'true'|
|Name of the backup file in Azure storage| `sapsec_test_adhoc_2026-10-15` - adjust for backup date, or use filename entered in (3) |

### 7. Disable maintenance page on prod
Run workflow: **Manage Website Maintenance Mode** (toggle-maintenance-page.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to update| `production`|
|Route traffic to the maintenance page or back to the app| `disable`|

## Post-release
### 1. Edit workflows to reinstate enabling/disabling maintenance page on prod
Undo `if:` conditions added in Pre-release (1)
### 2. Delete dummy data files from test blob storage
|Details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdptssa`|
|Blob container|`schooldata`|
### 3. Verify files in test blob storage by running data pipeline against test
Run workflow: **School Data Ingestion Pipeline** (data-pipeline.yml)
|Parameter|Value|
|-|-|
|Use workflow from| Branch: `main`|
|Environment to run against| `test`|
|Pull request number of the review app| leave blank |
|Ignore the rebuild list and rebuild all raw tables| checked |
|Optional override path to the raw tables rebuild list|leave blank|
### 4. Delete dummy data files in prod blob storage
|Details||
|-|-|
|Environment|Production|
|Storage account|`s189p01sapsecdppdsa`|
|Blob container|`schooldata`|
### 5. Upload released data files to prod blob storage
|Details||
|-|-|
|Environment|Production|
|Storage account|`s189p01sapsecdppdsa`|
|Blob container|`schooldata`|
### 6. Verify files in prod blob storage by visual inspection against test blob storage
|Test details||
|-|-|
|Environment|Test|
|Storage account|`s189t01sapsecdptssa`|
|Blob container|`schooldata`|

|Prod details||
|-|-|
|Environment|Production|
|Storage account|`s189p01sapsecdppdsa`|
|Blob container|`schooldata`|
