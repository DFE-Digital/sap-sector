# Sentry Monitoring

This service sends exception events to Sentry when the `Sentry` configuration section is enabled and a DSN is provided.

The implementation lives in `SAPSec.Web/Program.cs` and `SAPSec.Web/Configuration/SentryConfiguration.cs`.

## What is captured

- unhandled ASP.NET Core exceptions
- stack traces for captured exceptions
- Sentry environment tags for filtering and alert routing

The service currently defaults to:

- breadcrumb level: `Information`
- event level: `Error`
- PII disabled

## Environment behaviour

Sentry environment tags follow the ASP.NET Core hosting environment unless `Sentry:Environment` overrides them.

Terraform currently sets:

- review -> `ASPNETCORE_ENVIRONMENT=Development`
- test -> `ASPNETCORE_ENVIRONMENT=Test`
- production -> `ASPNETCORE_ENVIRONMENT=Production`

That means Sentry events are tagged as:

- review -> `development`
- test -> `test`
- production -> `production`

Automated test-hosted appsettings files keep Sentry disabled.

## Configuration

Base configuration is in `SAPSec.Web/appsettings.json`.

Environment overrides are in:

- `SAPSec.Web/appsettings.Development.json`
- `SAPSec.Web/appsettings.Test.json`
- `SAPSec.Web/appsettings.Production.json`

The DSN should be supplied as a secret, not committed to source control.

Supported configuration keys:

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

You can also provide the DSN with the `SENTRY_DSN` environment variable.

## Local development setup

Local development uses .NET user secrets.

This project already has a `UserSecretsId` in `SAPSec.Web/SAPSec.Web.csproj`.

To enable Sentry locally in `Development`, set:

```powershell
dotnet user-secrets --project .\SAPSec.Web\SAPSec.Web.csproj set "SENTRY_DSN" "<your-dsn>"
dotnet user-secrets --project .\SAPSec.Web\SAPSec.Web.csproj set "Sentry:Enabled" "true"
```

To confirm the values exist:

```powershell
dotnet user-secrets --project .\SAPSec.Web\SAPSec.Web.csproj list
```

Important notes:

- user secrets are local to your machine and are not committed to git
- user secrets are still readable in plain text by the local user account
- `Development` is disabled by default in `SAPSec.Web/appsettings.Development.json`, so the DSN alone is not enough; `Sentry:Enabled=true` is also required

## Recommended setup by environment

### Test

- keep `Sentry:Enabled` set to `true`
- provide the test-project DSN through `SENTRY_DSN` or `Sentry__Dsn`
- verify events appear under the `test` environment in Sentry

Example:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Test"
$env:SENTRY_DSN="<test-dsn>"
dotnet run --project .\SAPSec.Web\SAPSec.Web.csproj
```

### Production

- keep `Sentry:Enabled` set to `true`
- provide the production-project DSN through the deployment platform secret store

Example environment variables:

```text
ASPNETCORE_ENVIRONMENT=Production
SENTRY_DSN=<production-dsn>
```

### Development

Development is disabled by default to avoid local noise. To test locally, set:

```powershell
dotnet run --project .\SAPSec.Web\SAPSec.Web.csproj
```

If you prefer environment variables instead of user secrets:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:Sentry__Enabled="true"
$env:SENTRY_DSN="<test-dsn>"
dotnet run --project .\SAPSec.Web\SAPSec.Web.csproj
```

## Filtering in Sentry

Use Sentry's built-in filters in the Issues or Discover views:

- `environment:test`
- `environment:production`
- `level:error`

Useful examples:

- `environment:test level:error`
- `environment:production is:unresolved`
- `environment:production issue.priority:[high,medium]`

Because this service tags environments consistently, the same queries work across deployments.

## Alert check

To prove alerts are wired correctly:

1. Deploy with a valid DSN in the target environment.
2. Trigger a controlled exception in that environment.
3. Confirm the event appears in Sentry with the expected `environment` tag.
4. Confirm the linked alert rule opens or notifies the configured channel.

For a safe validation, use the Sentry project in the test environment first.

## Review checklist

- local user secrets are configured when testing in `Development`
- `SENTRY_DSN` is configured in the secret store for test and production
- events appear with the expected `test` or `production` environment
- stack traces are visible on captured exceptions
- the alert rule has been exercised in test
