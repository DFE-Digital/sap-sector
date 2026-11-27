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

**Authenticated pages**:

- `/find-a-school`, `/find-a-school/search`, `/find-a-school/suggest` - school search
- `/school/secondary/{urn}` and sub-pages (`ks4-headline-measures`, `attendance`, `view-similar-schools`)
- `/school/primary/{urn}` and sub-pages (`ks2`, `attendance`, `view-similar-schools`)
- `/ComparePerformance`

School search/comparison sits behind DfE Sign-in (OpenID Connect + MFA),
which can't be scripted end-to-end - a login step can't complete an MFA
challenge automatically. Two ways to exercise these pages instead:

- **`loadtest`-mode instance** (see [How the `LoadTest` mode works](#how-the-loadtest-mode-works)):
  an app instance you run yourself with a test auth bypass and a seeded
  PostgreSQL database. No login of any kind needed. Only works on an instance you
  start yourself, never a shared deployment.
- **Session-cookie reuse against a real environment** (see
  [Authenticated pages against a real environment](#authenticated-pages-against-a-real-environment)):
  sign in once, manually, through a real browser as a dedicated test/service
  account (completing MFA as normal), then replay that session across the
  load test. Works against `test` or a review app with real data - useful
  when you need results from real infrastructure, not just local database.

> **Note on the production-tier results.** The valid breaking-point results in
> [docs/testing/008-load-tests.md](../docs/testing/008-load-tests.md) were
> produced against a `test` deployment temporarily built with auth bypassed
> at the code level (not via session cookie), with the `test` database
> temporarily scaled to production spec. That is a deliberate, temporary
> state of a shared environment - see the caveats in that doc. It disables
> real authentication while deployed that way and must be reverted.

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

2. **Prepare environment variables (only needed for review apps, real-env
   auth, or Grafana Cloud runs):**

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
   dotnet run --launch-profile LoadTest
   ```

   On Windows PowerShell:

   ```powershell
   dotnet run --project SAPSec.Web --launch-profile LoadTest
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

## Reading the results: the stress staircase

The `stress` scenario is a **stepped staircase**. It ramps to each user level,
holds for 90 seconds so the p95 at that level is steady-state, then steps up -
climbing well past the old 150-user cap to find the actual breaking point.
Every request is tagged with the step it belongs to, and a threshold is
declared per step, so k6 puts the per-level figures directly into the summary
JSON. **This means the summary JSON alone contains the whole staircase - you
don't need the raw CSV to read it.**

`analyse.cjs` reads that summary and prints the staircase:

```bash
node analyse.cjs sap-sector-load-test-summary.json
```

It prints a table of requests, median, p95, max and failure rate per user
level, flags each as ok/BREACH against the p95 SLO, and reports the last level
inside the SLO and the first level to breach. It requires no dependencies
(plain Node). Use it after any `stress` run.

If a run only reached the early steps (e.g. you stopped it), `analyse.cjs`
lists the levels that got no traffic so it's obvious the run was partial
rather than the higher levels passing.

### Optional: raw per-request CSV

`test:loadtest:stress:trace` additionally writes a gzipped CSV
(`results.csv.gz`) with system tags trimmed to keep it manageable. You only
need this for per-second detail within a step or per-endpoint tail latency;
for operating targets the summary JSON is enough. The raw CSV can be large,
so prefer the plain `stress` run unless you specifically need it.

> **`test:loadtest:stress:short`** is currently identical to
> `test:loadtest:stress` (there is no duration override). To run a genuinely
> short check, start `stress` and stop it after a few minutes - k6 still
> writes the summary on interrupt, and `analyse.cjs` will show the early
> steps. (Do not shorten it with `--duration`: that overrides the scenario
> entirely and runs a single VU instead.)

## Authenticated pages against a real environment

For search, school pages and comparison results against a deployed
environment with normal authentication, reuse a dedicated DfE Sign-in session:

1. **Get a dedicated test/service DSI account first**, if one doesn't
   already exist - never use a real school leader's account for this. Sort
   this with whoever manages the DSI client for this service before doing
   anything else.

2. **Sign in once, manually, through a real browser** against the target
   environment (e.g. `test`), completing MFA as normal.

3. **Copy the session cookie.** In DevTools: Application (Chrome) or Storage
   (Firefox) > Cookies > find `SAPSec.Auth` > copy its value. This is a live
   credential-equivalent to a password - never commit it, never paste it
   into chat, PRs, or logs.

4. **Set it in `.env`:**

   ```
   SESSION_COOKIE=<paste the value here>
   ```

5. **Run the load test** against the same environment you signed into:

   ```bash
   cd load_testing
   npm run test:test-env:quick
   ```

   Any scenario/environment combination works the same way - `load-test.js`
   switches to the authenticated journey mix whenever `SESSION_COOKIE` is
   set, regardless of which environment it's pointed at (see
   `sap-sector/utils/auth.js`).

The session uses sliding expiration, so it stays alive for the length of a
run as long as requests keep flowing - but it will eventually expire and
need refreshing (repeat steps 2-4) for a later run.

Verify the URN pools in `sap-sector/data/school-urns.js` against the target
database before a stress run. Missing schools or phase data can produce 404s.

This still generates real load against shared infrastructure, so the same
rules apply as any other real-environment run - see
[Running against the test environment](#running-against-the-test-environment).

### Running the stress staircase against test

Use k6 directly to run the staircase against the deployed `test`
origin - as the production-tier results in
[008-load-tests.md](../docs/testing/008-load-tests.md) were produced - point
at the direct backend origin explicitly and coordinate with the team first
(see the WAF note below):

```bash
k6 run -e SCENARIO=stress -e ENVIRONMENT=test -e "TEST_URL=https://<direct-test-origin>" sap-sector/load-test.js
```

Set `SESSION_COOKIE` for normal DfE Sign-in or add `-e AUTH_OFF=true` if the
restricted Test deployment has the temporary bypass enabled. Run the
`quick` scenario first and check for failed requests and content checks.

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
`rate_limit_max` is **300 on test** and **1000 on production** (time window
unconfirmed - see 008-load-tests.md). Exceeding this will get the load-test
runner's IP blocked, not just throttled.

**The `stress` staircase deliberately exceeds these limits** (it climbs to
2,400 users to find the breaking point), so it must be run against the direct
backend origin with Front Door/WAF bypassed, as a team-cleared arrangement -
never through the public Front Door endpoint. The lighter `quick`/`baseline`
scenarios stay well under the limit.

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

The local `LoadTest` launch profile uses a synthetic identity in place of
DfE Sign-in and disables analytics middleware.
It still uses PostgreSQL. Seed a local database and provide
`ConnectionStrings__PostgresConnectionString` in your shell before launching.
Check that the URNs in `sap-sector/data/school-urns.js` exist in that database.

Use this launch profile locally. For shared environments, see the
[load test runbook](../docs/testing/008-load-tests.md) for the Test-only
bypass and the Review workflow using real DfE Sign-in.
## Test scenarios

| Scenario | Peak users | Purpose |
|---|---|---|
| `quick` | 10 | Smoke test |
| `baseline` | 10 | Normal operations |
| `peak-surge` | 50 | Surge event (e.g. results day) |
| `stress` | up to 2,400 (staircase, 90s hold per step) | Breaking-point identification |

Select via `--env SCENARIO=<name>` (defaults to `quick`). See the
[stress staircase](#reading-the-results-the-stress-staircase) section for how
`stress` is shaped and read.

## npm scripts

| Script | Environment | Scenario |
|---|---|---|
| `test:quick` / `:baseline` / `:peak` / `:stress` | `local` | quick / baseline / peak-surge / stress |
| `test:review:quick` / `:baseline` | `review` | quick / baseline |
| `test:test-env:quick` / `:baseline` | `test` | quick / baseline |
| `test:loadtest:quick` / `:baseline` / `:peak` / `:stress` | `loadtest` | quick / baseline / peak-surge / stress |
| `test:loadtest:stress:short` | `loadtest` | stress (currently same as `:stress` - see note above) |
| `test:loadtest:stress:trace` | `loadtest` | stress, plus a gzipped per-request CSV |
| `grafana:login` | - | Grafana Cloud auth |

## Running in CI

[`.github/workflows/loadtest.yml`](../.github/workflows/loadtest.yml) runs
this suite on demand from the Actions tab (`workflow_dispatch`), the same way
[publish-teacher-training's `loadtest.yml`](https://github.com/DFE-Digital/publish-teacher-training/blob/main/.github/workflows/loadtest.yml)
does - **it does not run automatically on every PR.** Pick a `scenario` and
target `environment` (`review` or `test`) when triggering it manually; for
`review` you also need to supply the PR number. It only covers the anonymous
journeys (same as running against `review`/`test` locally - see
[What's covered](#whats-covered)); it doesn't run against `production`, and
it doesn't set a `SESSION_COOKIE` or set up a `LoadTest`-mode instance, so
the authenticated pages aren't covered in CI (a live session cookie isn't
something to store as a long-lived CI secret - it's tied to one human's
sign-in and expires). Reports are uploaded as a workflow artifact.

We deliberately didn't wire this into `pull_request`: `test` sits behind a
rate-limited WAF shared with other traffic, and most PRs don't touch anything
performance-sensitive enough to justify load-testing every single one.

## Output

- **Local:** results printed to the terminal; `sap-sector-load-test-summary.json`
  and `sap-sector-load-test-report.html` are written to the working directory.
  Read a `stress` run with `node analyse.cjs sap-sector-load-test-summary.json`.
- **Cloud (Grafana):** run `npm run grafana:login` first (needs
  `K6_CLOUD_API_TOKEN`), then `k6 cloud run sap-sector/load-test.js` for
  real-time dashboards and historic tracking.

## Notes

- The latest Test stress runs met both targets through 100 virtual users
  at 200 and 300 connections per replica. See the
  [load test runbook](../docs/testing/008-load-tests.md) for the comparison
  and the historical 100-connection baseline.
- **Anonymous pages**, verified against the local `dotnet run` dev server: all
  requests succeed and content checks pass, but the homepage occasionally
  exceeds the 2000ms threshold under the `quick` scenario's 10-VU burst - this
  is the unoptimized Development-mode dev server (Kestrel + Razor JIT), not a
  real issue. Expect cleaner numbers against a review app, `test`, or a
  production-mode Docker build.
- See the [load test runbook](../docs/testing/008-load-tests.md) for the latest
  authenticated smoke and stress results.

## Structure

```
load_testing/
├── package.json
├── analyse.cjs            # reads a stress summary JSON into a per-level staircase table
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
│   ├── scenarios/          # load shapes (quick/baseline/peak-surge/stress staircase)
│   └── utils/
│       ├── checks.js       # response time + content assertions, custom metrics
│       └── auth.js         # SESSION_COOKIE handling for real-environment auth
└── shared/
    └── utils/
        └── common-helpers.js
```
