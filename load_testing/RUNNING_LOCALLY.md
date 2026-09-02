# Running the load tests locally

Quick-start for running the [SAP Sector k6 load tests](README.md) on your own
machine. See the main [load_testing README](README.md) for scenario details,
running against review/test environments, and CI.

## Prerequisites (once)

1. Clone the repo and check out this branch.
2. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and
   [Node.js 22.x](https://nodejs.org/) (same as the main [README](../README.md)
   prerequisites).
3. Install k6:

   *Windows*
   ```
   winget install k6 --source winget
   ```

   *macOS*
   ```
   brew install k6
   ```

   *Linux (Debian/Ubuntu)*
   ```
   sudo apt install k6
   ```

## Path A - anonymous pages only (simplest)

Covers the homepage, static pages, and health checks. No auth setup needed.

1. Start the app normally:

   ```bash
   cd SAPSec.Web
   dotnet run
   ```

2. In another terminal:

   ```bash
   cd load_testing
   npm run test:quick
   ```

Results print to the terminal, and `sap-sector-load-test-report.html` is
written to `load_testing/`.

## Path B - authenticated pages too (search, school pages, compare performance)

Needs the app run in a `LoadTest` mode we added, which bypasses DfE Sign-in
and uses fixture data instead of a real database - no credentials or DB setup
required. **Make sure your checkout includes the `LoadTest` changes to
`SAPSec.Web`** (`Program.cs`, `appsettings.LoadTest.json`,
`DfeAnalyticsExtensions.cs`) - on an older checkout without them you'll hit
real DSI/Postgres errors instead of the bypass.

1. One-time: trust the local HTTPS dev certificate, if you haven't already:

   ```bash
   dotnet dev-certs https --trust
   ```

2. Start the app in `LoadTest` mode over HTTPS:

   *macOS/Linux*
   ```bash
   cd SAPSec.Web
   ASPNETCORE_ENVIRONMENT=LoadTest ASPNETCORE_URLS=https://localhost:5099 dotnet run
   ```

   *Windows PowerShell*
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "LoadTest"
   $env:ASPNETCORE_URLS = "https://localhost:5099"
   dotnet run --project SAPSec.Web
   ```

3. In another terminal:

   ```bash
   cd load_testing
   npm run test:loadtest:quick
   ```

## Path C - authenticated pages against real data (test/review, not the fixture)

Path B uses JSON fixture data, not real Postgres data. If you need real
school data, sign in once manually (as a dedicated test/service account,
completing MFA) and reuse that session instead - see
[Authenticated pages against a real environment](README.md#authenticated-pages-against-a-real-environment)
in the main README for the full steps.

## Other scenarios

Once the `quick` smoke test looks good, swap it for a heavier one:

| Scenario | Anonymous (Path A) | Authenticated (Path B) |
|---|---|---|
| Smoke test | `npm run test:quick` | `npm run test:loadtest:quick` |
| Normal load | `npm run test:baseline` | `npm run test:loadtest:baseline` |
| Surge event | `npm run test:peak` | `npm run test:loadtest:peak` |
| Breaking point | `npm run test:stress` | `npm run test:loadtest:stress` |

See [Test scenarios](README.md#test-scenarios) in the main README for what
each one actually does (peak users, duration, purpose).

## Troubleshooting

- **500 error on any page with a form, Path B**: you're probably running over
  plain HTTP. The app's cookies require `Secure`, so `LoadTest` mode must be
  run over `https://`.
- **404 on `/school/primary/...`, Path B**: make sure you picked up
  `appsettings.LoadTest.json` - it turns on the `EnablePrimarySchools`
  feature flag, which is off by default.
- **Real DSI sign-in redirect instead of the bypass**: `ASPNETCORE_ENVIRONMENT`
  isn't set to exactly `LoadTest`, or your checkout predates the `LoadTest`
  mode changes.
