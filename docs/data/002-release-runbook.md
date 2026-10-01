# Data release runbook Oct 2026

## Local development
1. Copy dummy data files to local dev environment
2. Update data map to new files/columns
3. Run data pipeline against local DB
4. Regenerate DTOs and rebuild solution, resolve any build errors due to missing/updated properties
5. Test service locally, identify and resolve any data issues
6. Create data release PR and test in review app
7. Review and approve data release PR
8. (update data pipeline to remove unneeded data - check with public team)

## Pre-release
1. Edit workflows to skip enabling/disabling maintenance page on prod (identify + list)
1. Enable maintenance page on prod
2. Upload dummy data files to test blob storage
3. Run data pipeline against test DB
4. Ensure data release PR is up-to-date, tests passing, no merge conflicts
5. Resolve any issues in data release PR found in (4)
6. Halt all other PR merges to main
7. Merge data release PR
8. Test service on test environment, resolving any issues
   
## Release
1. Rename dummy data files in test blob storage
2. Upload released data files to test blob storage (ensuring same names as dummy data files)
3. Run data pipeline against test DB
4. Create backup of test DB (workflow, filename)
5. Download backup file from test (location)
6. Upload backup file to prod (location)
7. Restore backup file (workflow, filename)
8. Disable maintenance page on prod
9. Edit workflows to reinstate enabling/disabling maintenance page on prod
10. Rename existing data files in prod blob storage
11. Upload released data files to prod blob storage
