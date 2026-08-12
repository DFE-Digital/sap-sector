# SAP Sector - Load Testing

Load testing suite for the [SAP Sector](../README.md) (School Account Profile) service using [k6](https://grafana.com/products/k6/).

Structure follows the same pattern used by [DFE-Digital/publish-teacher-training's load_testing suite](https://github.com/DFE-Digital/publish-teacher-training/tree/main/load_testing), scaled down to a single service.

New to this? See **[RUNNING_LOCALLY.md](RUNNING_LOCALLY.md)** for a quick-start guide to running it on your own machine.

## What's covered

**Anonymous pages** (run against local/review/test/production):

- `/` - homepage
- `/accessibility`, `/cookies`, `/terms-and-conditions` - static content pages
- `/health`, `/healthcheck` - monitoring/liveness probes
- `/auth/signin` - checked only as a redirect boundary (302 to DfE Sign-in), never followed

**Authenticated pages** (run only against a `loadtest`-mode instance - see below):

- `/find-a-school`, `/find-a-school/search`, `/find-a-school/suggest` - school search
- `/school/secondary/{urn}` and sub-pages (`ks4-headline-measures`, `attendance`, `view-similar-schools`)
- `/school/primary/{urn}` and sub-pages (`ks2`, `attendance`, `view-similar-schools`)
- `/ComparePerformance`

School search/comparison sits behind DfE Sign-in (OpenID Connect), which can't
be scripted against a real deployment without user credentials - so these
pages are never exercised against local/review/test/production. Instead the
app has a `LoadTest` environment mode (added for this purpose, see
[How the `LoadTest` mode works](#how-the-loadtest-mode-works)) that swaps in a
test auth bypass and JSON-backed fixture data, and can only be run on an
instance you start yourself.

## Setup

1. **Install k6:**

   *macOS*
   ```
   brew install k6
   ```

   *Windows*
   ```
   winget install k6 --source winget
   ```

   *Linux (Debian/Ubuntu)*
   ```
   sudo apt install k6
   ```

2. **Prepare environment variables (only needed for review apps or Grafana Cloud runs):**

   ```
   cd load_testing
   cp .env.example .env
   # Edit .env as needed
   ```

## Running locally (anonymous pages)

Start the app normally (`dotnet run --project SAPSec.Web` or
`docker run -p 3000:3000 sapsec:latest`), then:

```bash
cd load_testing
npm run test:quick
npm run test:baseline
npm run test:peak
npm run test:stress
```

## Running locally (authenticated pages: search, school pages, compare)

1. **Start the app in `LoadTest` mode**, over HTTPS (the app's cookies require
   `Secure`, so plain HTTP will 500 on any page with a form). From the repo
   root:

   ```bash
   dotnet dev-certs https --trust   # once, if you haven't already
   cd SAPSec.Web
   ASPNETCORE_ENVIRONMENT=LoadTest ASPNETCORE_URLS=https://localhost:5099 dotnet run
   ```

   On Windows PowerShell:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "LoadTest"
   $env:ASPNETCORE_URLS = "https://localhost:5099"
   dotnet run --project SAPSec.Web
   ```

2. **Run the load test** against it:

   ```bash
   cd load_testing
   npm run test:loadtest:quick
   npm run test:loadtest:baseline
   npm run test:loadtest:peak
   npm run test:loadtest:stress
   ```

   `LOADTEST_URL` in `.env` overrides the default `https://localhost:5099` if
   you run the app on a different port.

When `ENVIRONMENT=loadtest`, `load-test.js` swaps in the authenticated
journey mix (search, secondary/primary school pages, compare performance)
instead of the anonymous one - see
[How the `LoadTest` mode works](#how-the-loadtest-mode-works).

## Running against a PR review app

Set `PR_NUMBER` (or `REVIEW_URL` directly) in `.env`, then:

```
npm run test:review:quick
npm run test:review:baseline
```

## Running against the test environment

**Coordinate with the team before doing this.** The test and production
environments sit behind Azure Front Door with WAF rate limiting and IP
blocking enabled (see `terraform/domains/environment_domains/config/`):
`rate_limit_max` is **300 req/s on test** and **1000 req/s on production**.
Exceeding this will get the load-test runner's IP blocked, not just
throttled. The `stress` scenario is deliberately capped at 150 concurrent
users to stay well under this, but check with the team first regardless.

```
npm run test:test-env:quick
npm run test:test-env:baseline
```

Production is intentionally not wired up as an npm script. If you need to
load test production, agree it with the team first and run it explicitly:

```
k6 run --env SCENARIO=quick --env ENVIRONMENT=production sap-sector/load-test.js
```

## How the `LoadTest` mode works

Two small, additive changes to `SAPSec.Web` make the `LoadTest` environment
name behave like the app's existing `UITests`/`IntegrationTests`/
`EndToEndTests`/`AccessibilityTests` environments, which were already used by
the UI/E2E/integration test suites to bypass real auth:

- [`Program.cs`](../SAPSec.Web/Program.cs) - added `"LoadTest"` to the
  environment-name check that swaps DfE Sign-in for `AutoAuthenticationHandler`
  (auto-authenticates every request as a fake test user, no login needed), and
  added a guarded block that calls `AddJsonDependencies()` after
  `AddPostgresqlDependencies()` when `EnvironmentName == "LoadTest"` - this
  swaps the Postgres-backed repositories for the same JSON-file-backed ones
  (`SAPSec.Infrastructure/Data/Files/Generated/*.json`, ~1,924 real-looking
  DfE-style establishment records) already used by the integration test suite.
  **Every other environment name is untouched and keeps using Postgres.**
- [`appsettings.LoadTest.json`](../SAPSec.Web/appsettings.LoadTest.json) -
  turns on the `EnablePrimarySchools` feature flag, matching the other test
  environments' overlay files (it's off by default in `appsettings.json`).
- [`DfeAnalyticsExtensions.cs`](../SAPSec.Web/Extensions/DfeAnalyticsExtensions.cs) -
  added `"LoadTest"` to the environments that skip sending events to real
  Google Analytics/Clarity, so load-test traffic doesn't pollute production
  analytics.

**This must never be set on a shared review/test/production deployment** -
`LoadTest` disables real DfE Sign-in authentication entirely. It's only safe
because it's a distinct, explicit environment name that has to be deliberately
opted into; nothing sets it by default.

### Known-good test data

The JSON fixture doesn't have full data for every URN, so
[`sap-sector/data/school-urns.js`](sap-sector/data/school-urns.js) contains
URN pools individually verified (via `curl`, against a running `LoadTest`
instance) to return 200 from their overview and sub-pages: 30 secondary
school URNs and 28 primary school URNs, drawn from
`SAPSec.Infrastructure/Data/Files/TestEstablishmentUrns.json`. Search query
terms (`School`, `High`, `Primary`, `Academy`) and suggest prefixes were
verified the same way to return real results.

## Test scenarios

| Scenario | Peak users | Duration | Purpose |
|---|---|---|---|
| `quick` | 10 | 25s | Smoke test |
| `baseline` | 10 | ~4m | Normal operations |
| `peak-surge` | 50 | ~5.5m | Surge event (e.g. results day) |
| `stress` | 150 | ~9m | Breaking point identification |

Select via `--env SCENARIO=<name>` (defaults to `quick`).

## Running in CI

[`.github/workflows/loadtest.yml`](../.github/workflows/loadtest.yml) runs
this suite on demand from the Actions tab (`workflow_dispatch`), the same way
[publish-teacher-training's `loadtest.yml`](https://github.com/DFE-Digital/publish-teacher-training/blob/main/.github/workflows/loadtest.yml)
does - **it does not run automatically on every PR.** Pick a `scenario` and
target `environment` (`review` or `test`) when triggering it manually; for
`review` you also need to supply the PR number. It only covers the anonymous
journeys (same as running against `review`/`test` locally - see
[What's covered](#whats-covered)); it doesn't run against `production`, and
it doesn't set up a `LoadTest`-mode instance to exercise the authenticated
pages. Reports are uploaded as a workflow artifact.

We deliberately didn't wire this into `pull_request`: `test` sits behind a
rate-limited WAF shared with other traffic, and most PRs don't touch anything
performance-sensitive enough to justify load-testing every single one.

## Output

- **Local:** results printed to the terminal; `sap-sector-load-test-summary.json`
  and `sap-sector-load-test-report.html` are written to the working directory.
- **Cloud (Grafana):** run `npm run grafana:login` first (needs
  `K6_CLOUD_API_TOKEN`), then `k6 cloud run sap-sector/load-test.js` for
  real-time dashboards and historic tracking.

## Notes

- **Anonymous pages**, verified against the local `dotnet run` dev server: all
  requests succeed and content checks pass, but the homepage occasionally
  exceeds the 2000ms threshold under the `quick` scenario's 10-VU burst - this
  is the unoptimized Development-mode dev server (Kestrel + Razor JIT), not a
  real issue. Expect cleaner numbers against a review app, `test`, or a
  production-mode Docker build.
- **Authenticated pages**, verified against a local `LoadTest`-mode instance
  (`quick` scenario, 10 VUs): 383/383 checks passed, 0% error rate, across
  all four journeys (search, secondary school, primary school, compare
  performance) - see [How the `LoadTest` mode works](#how-the-loadtest-mode-works).
- Docker wasn't available in the environment this was built in, so a
  Postgres-backed local run (real DB instead of the JSON fixture) hasn't been
  verified - only the JSON-backed `LoadTest` mode has.

## Structure

```
load_testing/
├── package.json
├── .env.example
├── sap-sector/
│   ├── load-test.js       # entry point: scenario selection, journey mix, reporting
│   ├── config/
│   │   └── environment.js # base URLs and thresholds per ENVIRONMENT (incl. loadtest)
│   ├── data/
│   │   └── school-urns.js # verified-working primary/secondary URN pools
│   ├── journeys/
│   │   ├── homepage.js              # anonymous
│   │   ├── static-pages.js          # anonymous
│   │   ├── health-check.js          # anonymous
│   │   ├── sign-in-redirect.js      # anonymous
│   │   ├── school-search.js         # authenticated (loadtest only)
│   │   ├── school-overview.js       # authenticated (loadtest only)
│   │   └── compare-performance.js   # authenticated (loadtest only)
│   ├── scenarios/          # load shapes (quick/baseline/peak-surge/stress)
│   └── utils/
│       └── checks.js       # response time + content assertions, custom metrics
└── shared/
    └── utils/
        └── common-helpers.js
```
