# Restore test database to production

Use these GitHub Actions workflows when a test database backup may need to be restored into production later:

1. `Backup database to Azure storage`
2. `Restore test database to production`

This is a destructive production operation. It replaces the production database contents with the selected test backup.

## Prerequisites

- GitHub production environment approval is available.
- If `backup-storage-location` is `test`, the production Azure identity used by the workflow can read the test backup storage account and write to the production backup storage account.
- The production application impact has been agreed with the service owner.

## Create the test backup

1. Open GitHub Actions.
2. Select `Backup database to Azure storage`.
3. Choose `Run workflow`.
4. Set `environment` to `test`.
5. Enter a clear `backup-file` value without an extension, for example `sapsec_ts_to_prod_2026-10-01`.
6. Start the workflow.
7. Copy the backup file name from the workflow summary. It will end in `.sql.gz`.

This step can be run hours before the production restore.

## Restore to production

1. Open GitHub Actions.
2. Select `Restore test database to production`.
3. Choose `Run workflow`.
4. Set `confirm-production` to `true`.
5. Enter the restore reason.
6. Enter the `test-backup-file` from the earlier test backup workflow. It must end in `.sql.gz`.
7. Set `backup-storage-location`:
   - `test` when the backup is still in the test backup storage account.
   - `production` when the backup has already been manually copied into the production `database-backup` container.
8. Start the workflow and approve the production environment when prompted.

The production restore workflow will:

1. validate the confirmation and restore reason;
2. confirm the named backup exists in the selected storage location;
3. create a production backup before changing production;
4. copy the test backup into the production `database-backup` storage container when needed;
5. restore production from the selected backup;
6. write the restored test backup name and production rollback backup name to the workflow summary.

## Rollback

Use the `Restore database from Azure storage` workflow with:

- `environment`: `production`
- `confirm-production`: `true`
- `backup-file`: the production pre-restore backup file shown in the `Restore test database to production` workflow summary

## Validation

After restore, check:

- the production app health endpoint;
- key production user journeys;
- database migration/version state;
- Sentry and application logs;
- a small sample of expected production-visible data.
