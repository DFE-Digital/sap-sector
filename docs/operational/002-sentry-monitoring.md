# Sentry Monitoring

This repository sends exceptions to Sentry from two places:

- `SAPSec.Web`, the ASP.NET Core web application
- `SAPData`, the data pipeline console application used by the GitHub Actions ingestion workflow

The main implementation lives in:

- `SAPSec.Web/Program.cs`
- `SAPSec.Web/Configuration/SentryConfiguration.cs`
- `SAPData/Program.cs`
- `.github/workflows/data-pipeline.yml`
- `terraform/application/application.tf`

## What is captured

### Web application

When Sentry is enabled and a DSN is configured, the web app captures:

- unhandled ASP.NET Core exceptions
- stack traces
- Sentry environment tags

The web app currently uses `Sentry.AspNetCore` via `UseSentry(...)`.
Serilog is still configured for console/debug logging only and is not forwarded to Sentry in this change set.

Web defaults:

- breadcrumb level: `Information`
- event level: `Error`
- PII disabled

### Data pipeline

The `SAPData` console app initializes Sentry at startup and captures:

- unhandled pipeline exceptions
- stack traces
- Sentry environment tags derived from `DEPLOY_ENV`, `DOTNET_ENVIRONMENT`, or `ASPNETCORE_ENVIRONMENT`

Pipeline defaults:

- PII disabled
- stack traces enabled

## Environment behaviour

### Web application

Web Sentry environment tags follow the ASP.NET Core hosting environment unless `Sentry:Environment` is explicitly configured.

Terraform currently sets:

- review -> `ASPNETCORE_ENVIRONMENT=Development`
- test -> `ASPNETCORE_ENVIRONMENT=Test`
- production -> `ASPNETCORE_ENVIRONMENT=Production`

That means Sentry events are typically tagged as:

- review -> `development`
- test -> `test`
- production -> `production`

Automated web test-hosted appsettings files keep Sentry disabled.

### Data pipeline

The GitHub Actions data pipeline sets:

- `SENTRY_DSN`
- `Sentry__Enabled=true`
- `DEPLOY_ENV=test` or `DEPLOY_ENV=production`

That means pipeline events are tagged as:

- test -> `test`
- production -> `production`

## Configuration

### Web application

Base configuration is in `SAPSec.Web/appsettings.json`.

Environment overrides are in:

- `SAPSec.Web/appsettings.Development.json`
- `SAPSec.Web/appsettings.Test.json`
- `SAPSec.Web/appsettings.Production.json`

Supported keys:

```json
"Sentry": {
  "Enabled": true,
  "Dsn": "",
  "Environment": "",
  "Debug": false,
  "MinimumBreadcrumbLevel": "Information",
  "MinimumEventLevel": "Error"
}
```

The DSN can also be supplied via `SENTRY_DSN`.

### Data pipeline

`SAPData` reads:

- `Sentry:Enabled` from configuration or `Sentry__Enabled` from the environment
- `Sentry:Dsn` from configuration or `SENTRY_DSN` from the environment
- environment name from `DEPLOY_ENV`, `DOTNET_ENVIRONMENT`, or `ASPNETCORE_ENVIRONMENT`

The pipeline does not require an appsettings file for Sentry.

## Infrastructure wiring

### Web runtime

Terraform reads the `SentryDsn` secret from Key Vault and exposes it to the application as `SENTRY_DSN`.

The expected Key Vault secret name is exactly `SentryDsn`.
The deployment/runtime identity that reads application secrets must have permission to read that secret.

### Data pipeline workflow

The GitHub Actions workflow `.github/workflows/data-pipeline.yml` passes:

- `SENTRY_DSN: ${{ secrets.SENTRY_DSN }}`
- `Sentry__Enabled: "true"`

For the pipeline to report exceptions, a `SENTRY_DSN` secret must exist in the GitHub environment used by the workflow.

## Local development setup

### Web application

Local web development uses .NET user secrets.

This project already has a `UserSecretsId` in `SAPSec.Web/SAPSec.Web.csproj`.

To configure a local DSN:

```powershell
dotnet user-secrets --project .\SAPSec.Web\SAPSec.Web.csproj set "SENTRY_DSN" "<your-dsn>"
```

To confirm the value exists:

```powershell
dotnet user-secrets --project .\SAPSec.Web\SAPSec.Web.csproj list
```

`SAPSec.Web/appsettings.Development.json` already enables Sentry, so a valid local DSN is enough.

You can also use environment variables instead of user secrets:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:SENTRY_DSN="<your-dsn>"
dotnet run --project .\SAPSec.Web\SAPSec.Web.csproj
```

### Data pipeline

For manual local runs of `SAPData`, you can set environment variables in the shell before running the project:

```powershell
$env:Sentry__Enabled="true"
$env:SENTRY_DSN="<your-dsn>"
$env:DEPLOY_ENV="test"
dotnet run --project .\SAPData\SAPData.csproj
```

## Local verification

### Web application

The development environment exposes two anonymous endpoints:

- `/dev/sentry-message`
- `/dev/sentry-exception`

Use `/dev/sentry-message` to verify basic event delivery.

Use `/dev/sentry-exception` to verify real exception capture.

Typical local flow:

```powershell
dotnet run --project .\SAPSec.Web\SAPSec.Web.csproj
```

Then open:

- `https://localhost:<port>/dev/sentry-message`
- `https://localhost:<port>/dev/sentry-exception`

Expected result:

- `/dev/sentry-message` creates a message event in Sentry
- `/dev/sentry-exception` creates an exception event in Sentry

### Data pipeline

For pipeline verification, run `SAPData` locally with a valid DSN and trigger a controlled failure.

The process should exit with an exception and the exception should appear in Sentry with the expected environment tag.

## Recommended setup by environment

### Test

- keep web `Sentry:Enabled` set to `true`
- provide the test DSN through Key Vault for the web app
- provide the test `SENTRY_DSN` GitHub environment secret for the data pipeline
- verify events appear under `environment:test`

### Production

- keep web `Sentry:Enabled` set to `true`
- provide the production DSN through Key Vault for the web app
- provide the production `SENTRY_DSN` GitHub environment secret for the data pipeline
- verify events appear under `environment:production`

### Review / Development

- review deployments inherit `Development`
- local web development needs a DSN to emit events
- local `SAPData` runs need `Sentry__Enabled` and `SENTRY_DSN`

## Filtering in Sentry

Useful filters:

- `environment:test`
- `environment:production`
- `level:error`

Examples:

- `environment:test level:error`
- `environment:production is:unresolved`
- `environment:test transaction:SAPData`

## Alert check

To prove alerts are wired correctly:

1. Configure a valid DSN in the target environment.
2. Trigger a controlled exception.
3. Confirm the event appears in Sentry with the expected `environment` tag.
4. Confirm the linked alert rule opens or notifies the configured channel.

For the web app, prefer `/dev/sentry-exception` in a non-production environment first.

For the data pipeline, prefer the `test` GitHub Actions environment first.

## Review checklist

- local web DSN is configured when testing in `Development`
- `SentryDsn` exists in Key Vault for deployed web environments
- `SENTRY_DSN` exists in the GitHub environment secrets for the data pipeline
- web events appear with the expected `development`, `test`, or `production` environment
- pipeline events appear with the expected `test` or `production` environment
- stack traces are visible on captured exceptions
- the alert rule has been exercised in test
