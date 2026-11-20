# Load Testing

Load testing verifies the service can handle expected and peak usage, using
[k6](https://k6.io/) - see [load_testing/](../../load_testing/README.md) for
the full test suite and [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for how to run it.

## Status

### Increased-pool retest preparation

The application now reads `Database:MaxPoolSize` (default 100) and optionally
`Database:ConnectionTimeoutSeconds` (otherwise the connection string/driver
timeout is preserved). The singleton `NpgsqlDataSourceFactory` lazily creates
and reuses one data source, so repositories share its connection pool.

Test, Review and Production default to 100 connections per app replica.
Terraform supports temporary 200 or 300 overrides in Test and Review.
Normal deployments use DfE Sign-in and retain the configured driver timeout.

To reproduce the historical synthetic authentication setup on a restricted
Test deployment, explicitly set the Terraform input
`load_testing_bypass_authentication` to `true`. This enables test
authentication, disables DfE Analytics and sets a 60-second connection
timeout while retaining real Postgres repositories. The flag defaults to
false and is applied only to Test. Restore it to false after the run.

For a temporary deployment override in PowerShell, set
`$env:TF_VAR_load_testing_bypass_authentication = 'true'` for the Test
deployment. To restore DfE Sign-in, set it to `'false'` (or remove the
override) and redeploy. If a tfvars file sets the input, change it there
instead because tfvars takes precedence. Restart or roll out all pods if
they still have the old ConfigMap environment variables. Verify that a
signed-out request to `/find-a-school` redirects to sign-in after restoration.
The corresponding application setting is `LoadTesting__BypassAuthentication`.
The workflow's `auth-bypassed` input and k6's `AUTH_OFF` flag just select the
authenticated journeys. They do not change server authentication.

In the **Build and Deploy** manual workflow, select the branch containing
these changes, choose `test`, and supply an image tag built from that branch.
Set `load-test-pool-size` to the desired size and select
`load-test-auth-bypass` only for a restricted Test run. To restore the
normal deployment, choose 100 and clear the bypass checkbox.

The Test stress runs at 200 and 300 connections per replica both met the
latency and HTTP failure targets through 100 virtual users. The historical
100-connection results below are retained for context. Their proposed
operating targets need reassessment in light of the newer runs.

### Compare 200 and 300 connections per replica

1. Deploy the same app build to Test for both runs. Set the Terraform input
   `load_testing_database_max_pool_size` to `200` or `300` in
   `terraform/application/config/test.tfvars.json`, or supply it via
   `TF_VAR_load_testing_database_max_pool_size` if the file does not set it.
   The default is 100. At two replicas the respective caps are 400 and 600
   against Test's previously observed database limit of 856. Recheck that
   limit and leave capacity for rolling deployments and other consumers.
2. After each rollout, verify the pool size on all ready pods. Keep the
   database tier, replica count, connection timeout, target URL, and k6
   scenario the same for both runs. Use the same Test origin if bypassing
   Front Door rate limits as in the historical benchmark.
3. Run each size from a separate output directory. The PowerShell example
   below assumes synthetic authentication was explicitly enabled on Test.
   With normal DfE Sign-in, omit `-e AUTH_OFF=true` and use a dedicated test
   account's session as described in the [k6 guide](../../load_testing/README.md#authenticated-pages-against-a-real-environment).

   ```powershell
   $pool = 200
   $testOrigin = 'https://<direct-test-origin>'
   $script = (Resolve-Path 'load_testing/sap-sector/load-test.js').Path
   $analyse = (Resolve-Path 'load_testing/analyse.cjs').Path
   $output = Join-Path ([System.IO.Path]::GetTempPath()) ("sap-sector-pool-$pool-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
   New-Item -ItemType Directory -Path $output | Out-Null
   Push-Location $output
   try {
       k6 run -e ENVIRONMENT=test -e "TEST_URL=$testOrigin" -e AUTH_OFF=true -e SCENARIO=stress $script
       $k6ExitCode = $LASTEXITCODE
       if (Test-Path sap-sector-load-test-summary.json) {
           node $analyse sap-sector-load-test-summary.json
       }
       Write-Output "k6 exit code: $k6ExitCode. Reports: $output"
   } finally {
       Pop-Location
   }
   ```

4. Record the UTC run window, confirmed pool setting, app commit, per-step
   p95 and failures, peak PostgreSQL connections (`pg_stat_activity` and
   Azure metrics), replica counts, and pod restarts. Restore the intended
   Test pool setting and disable the temporary auth bypass after both runs.

| Pool per replica | Shared Test stress run | Last step within SLO | First step outside SLO | Peak DB connections |
|---|---|---|---|---|
| 200 | 20 November 2026 rerun | 100 VUs | 150 VUs (latency) | Not captured |
| 300 | 20 November 2026 run | 100 VUs | 150 VUs (latency) | Not captured |

### Test stress rerun with a 200-connection pool · 20 November 2026

The run targeted the direct Test origin at
`get-school-improvement-insights-test.test.teacherservices.cloud` with a
confirmed pool cap of **200 per replica**.
It completed the 650-user hold and stopped at 758 active virtual users
during the ramp to 850, when the overall HTTP failure rate reached 5.021%
and crossed k6's 5% abort threshold.

| Virtual users | Hold window (elapsed) | Hold observed | Requests | Approx. requests/sec | Change | p95 | Failed requests |
|---:|---|---:|---:|---:|---:|---:|---:|
| 50 | 0:30–2:00 | 90 s | 1,821 | 20.2 | — | 1,355 ms | 0 (0.00%) |
| 100 | 2:30–4:00 | 90 s | 3,787 | 42.1 | +108.0% | 843 ms | 0 (0.00%) |
| 150 | 4:30–6:00 | 90 s | 3,690 | 41.0 | −2.6% | 5,204 ms | 0 (0.00%) |
| 250 | 6:30–8:00 | 90 s | 3,409 | 37.9 | −7.6% | 14,888 ms | 1 (0.03%) |
| 450 | 8:30–10:00 | 90 s | 1,785 | 19.8 | −47.6% | 60,004 ms | 705 (39.50%) |
| 650 | 10:30–12:00 | 90 s | 2,357 | 26.2 | +32.0% | 46,429 ms | 144 (6.11%) |

Throughput is the number of requests tagged to each hold divided by its
90-second duration. Requests from a journey that crosses a stage boundary
keep the tag from the start of that journey.

Across ramps and holds, k6 recorded **23,483 requests at 31.9 requests/sec**,
with an overall p95 of **40.2 seconds**. The console logged **1,118 request
failures without an HTTP response**, mostly timeouts, and **61 HTTP 500
responses**. No HTTP 400 or 404 responses were logged. Peak database
connections were not captured, so the cause of the timeouts remains
unconfirmed.

The rerun passed both targets at 50 and 100 users. At 150 users, p95
exceeded 3 seconds even though no requests failed. At 450 users, failures
exceeded the 1% target. The run did not measure recovery.

Generate the per-hold results from the saved k6 summary with:

```powershell
node load_testing/analyse.cjs "<report-directory>/sap-sector-load-test-summary.json"
```

### Test stress run with a 300-connection pool · 20 November 2026

The run used the same direct Test origin with a pool cap of **300 per
replica**. It reached the 850-user hold and stopped about 52 seconds into
that hold when k6's overall HTTP failure rate crossed the 5% abort threshold.

| Virtual users | Hold window (elapsed) | Hold observed | Requests | Approx. requests/sec | Change | p95 | Failed requests |
|---:|---|---:|---:|---:|---:|---:|---:|
| 50 | 0:30–2:00 | 90 s | 2,025 | 22.5 | — | 355 ms | 0 (0.00%) |
| 100 | 2:30–4:00 | 90 s | 3,817 | 42.4 | +88.5% | 786 ms | 0 (0.00%) |
| 150 | 4:30–6:00 | 90 s | 3,762 | 41.8 | −1.4% | 5,719 ms | 0 (0.00%) |
| 250 | 6:30–8:00 | 90 s | 3,714 | 41.3 | −1.3% | 16,850 ms | 4 (0.11%) |
| 450 | 8:30–10:00 | 90 s | 3,126 | 34.7 | −15.8% | 60,002 ms | 359 (11.48%) |
| 650 | 10:30–12:00 | 90 s | 2,381 | 26.5 | −23.8% | 60,002 ms | 583 (24.49%) |
| 850 | 12:30–13:22 | ~52 s (partial) | 1,335 | 25.9 | −2.3% | 36,913 ms | 469 (35.13%) |

K6 recorded **28,416 requests at 35.4 requests/sec**, an overall p95 of
**51.5 seconds**, and **2,035 failed requests (7.16%)**. The console logged
1,275 request timeouts, 2 HTTP/2 stream errors without a response, and
758 HTTP errors (710 HTTP 502, 41 HTTP 500, and 7 HTTP 504). No HTTP 400
or 404 responses were logged. Peak database connections and recovery
were not measured.

Both pool sizes met the SLO through **100 virtual users** and first
breached it on latency at **150**. At 450 users, the 300 pool handled more
requests per second and had fewer failures. At 650 users, it handled
roughly the same throughput but had more failures. These runs do not
show a consistent capacity improvement from raising the pool to 300.
Use ingress, pod and database metrics to investigate the timeouts and
502s before selecting a larger pool.

### Use a Review app for comparison

Use a dedicated PR Review app and set the same Terraform pool input for
each deployment. A change to the shared Review tfvars can affect other PR
deployments, so pass the override only to the dedicated deployment.
For anonymous smoke coverage run `k6 run -e ENVIRONMENT=review -e PR_NUMBER=<number> -e SCENARIO=quick load_testing/sap-sector/load-test.js`
from the repo root. For authenticated journeys, use the dedicated test
account's session for that Review URL and leave `AUTH_OFF` unset. For a
custom Review origin, supply `REVIEW_URL` instead of `PR_NUMBER`.

Start with a smoke run and confirm the database is seeded with the schools
used by k6. Before running stress, check the Review database's actual
`max_connections` and deployment replica count. Budget connections for
other consumers and rolling updates. Review uses different database
infrastructure from Test, so use its results to compare pool sizes within
the same Review app and use Test for production sizing. Keep Production
on its normal configuration and use smoke coverage there.

## Historical baseline with a 100-connection pool

Two staircase runs against the deployed `test` environment with real
authentication bypassed and a real database:

1. An initial run against `test` on its default **Burstable** database tier,
   which broke at ~250 concurrent users - a limit of the undersized database,
   not the service.
2. A second run after `test`'s database was scaled to **production spec**
   (General Purpose D2ds_v5, 2 vCores, 8 GiB). These results are
   production-representative and are the ones cited below - see
   [Production-tier stress result](#production-tier-stress-result).

In the historical production-tier run, the service met the latency target
through 150 concurrent users. Latency climbed through 850 users, and
failures became significant at 1,001. Infra identified the application's
PostgreSQL connection pool as a constraint in that run. See
[Why it breaks: connection pool](#why-it-breaks-connection-pool).

The operating targets below are proposed starting points, not yet signed
off. See [Open actions](#open-actions).

## Hosting model

The application runs on Azure Kubernetes Service (AKS):

- Behind Azure Front Door, with WAF rate limiting enabled -
  `rate_limit_max` is **300 on test** and **1000 on production**, with
  `block_ip: true` on repeat offenders
  (`terraform/domains/environment_domains/config/{test,production}.tfvars.json`).
  This is a real, already-in-place traffic-shedding mechanism. The runs below
  targeted the direct backend origin, bypassing Front Door/WAF, so the app
  could be characterised separately from infra-level shedding.
- Health probes on `/healthcheck` (liveness/readiness).
- A **fixed replica count of 2 pods** in both test and production
  (`terraform/application/config/{test,production}.tfvars.json`). During the
  production-tier run the two app pods scaled briefly to more replicas under
  load before settling; the platform team should confirm whether an HPA is
  configured, as the AKS module is vendored and not inspectable in this repo.
- **Database:** Azure Database for PostgreSQL Flexible Server. Production runs
  General Purpose D2ds_v5 (2 vCores, 8 GiB, HA enabled). Test's default is a
  Burstable B1ms (1 vCore, 2 GiB, no HA), which Azure marks development-only;
  it was temporarily scaled to match production for the valid run.

## What's tested

Two journey sets, covering both the anonymous and DfE Sign-in-authenticated
parts of the service:

**Anonymous** (run against any environment): homepage, static content pages
(accessibility/cookies/terms and conditions), health checks, and the DfE
Sign-in redirect boundary (checked as a 302, never followed).

**Authenticated** (school search, primary/secondary school performance
pages, compare performance): DfE Sign-in (OpenID Connect) can't be scripted
against a real deployment without live user credentials. These journeys
instead run against a dedicated `LoadTest` application mode
(`ASPNETCORE_ENVIRONMENT=LoadTest`) that swaps DfE Sign-in for an
auto-authenticating test scheme - see `SAPSec.Web/Program.cs` and
[How the `LoadTest` mode works](../../load_testing/README.md#how-the-loadtest-mode-works).
This mode only ever runs on a machine we control, or on a `test` deployment
temporarily and deliberately built with auth bypassed; it must never be the
normal state of a shared deployment, since it disables real authentication.

## Load scenarios

The `stress` scenario is a **stepped staircase**: it ramps to each level,
holds for 90s so the p95 at that level is steady-state, then steps up. Each
step is tagged so per-level figures come straight out of the summary JSON.
It climbs well past the earlier 150-VU cap to find the actual breaking point.

| Scenario | Peak concurrent users | Purpose |
|---|---|---|
| `quick` | 10 | Smoke test |
| `baseline` | 10 | Normal operations |
| `peak-surge` | 50 | Surge event (e.g. results day) |
| `stress` | up to 2,400 (staircase) | Breaking-point identification |

Thresholds: p95 response time under 3s, error rate under 1%, no 5xx
responses.

## Production-tier stress result

`stress` staircase against the deployed `test` environment, database scaled
to production spec (D2ds_v5), real authentication bypassed at the code level,
direct backend origin (Front Door/WAF bypassed). Per-level figures are
steady-state during each 90s hold. The p95 SLO is 3s.

| Concurrent users | p95 | Errors | Verdict |
|---|---|---|---|
| 50 | 499ms | 0% | fast |
| 100 | 475ms | 0% | fast |
| 150 | 790ms | 0% | fast, comfortable - last level inside SLO |
| 250 | 5,091ms | 0% | slow, still succeeding |
| 450 | 16,734ms | 0% | slow, still succeeding |
| 650 | 24,226ms | 0% | slow, still succeeding |
| 850 | 39,583ms | 0.02% | very slow, effectively no failures |
| 1,001 | 60,007ms | 6.26% | first real failures |
| 1,400 | 60,008ms | 47.45% | collapse |
| 1,800 | (run aborted) | 45% | `abortOnFail` on failure rate halted the run |

**Reading it.** Throughput plateaus at roughly 65 requests/second from ~150
users onward, regardless of how many more users are added - the classic
saturation signature. Beyond ~150 the extra load queues rather than getting
served, which is what drives p95 from sub-second to the 60s request timeout.
The service **degrades gradually and never sheds cleanly**: latency climbs
long before any request fails, and it recovered on its own once load eased.
The run aborted itself at ~1,800 users when the failure-rate threshold
tripped, which is by design.

The **application tier stayed light** throughout - app pods peaked around 1
of 4 available CPU cores (~25%), memory well within limits. The ceiling is
not app CPU or memory.

## Why it breaks: connection pool

Infra traced the failures and pod restarts at the top of the staircase to
**exhaustion of the application's Npgsql (PostgreSQL driver) connection
pool**, which is currently **100 connections per replica** (confirmed by
infra). Under load the pool filled; once empty, requests blocked waiting for a
connection.

The pod restart *mechanism* is still being confirmed with infra - two signals
appeared at different points in the run and may be two separate effects:

- **Liveness-probe timeouts** at 17:15-17:17 (kubelet log: `failed liveness
  probe, will be restarted`) - consistent with the health check being one of
  the requests blocked on an exhausted pool.
- **Exit Code 137** at ~18:16, an hour later - typically an OOM kill (pod
  hitting its memory limit), though 137 only strictly means the container was
  SIGKILLed. Whether the pods hit their configured memory limit, and what that
  limit is, is being checked with infra.

Both replicas were affected together because they share the same database and
the same pool limit. Either way the brief 502/504s from the ingress are
explained by requests hitting pods mid-restart.

Crucially, **the database itself was healthy throughout with capacity to
spare** - its own metrics showed only 2 failed connections across the whole
run, with succeeded connections peaking around 150. The Test database's
`max_connections` is **856**, while the application can only ever open
**2 replicas × 100 = 200** connections at its current pool size - so the
database had over 650 connections spare when the service began failing. The
constraint is an application configuration value (the pool cap), not database
hardware.

The historical run first saw significant failures at about 1,001 users
with a pool of 100 per replica. The later 200 and 300 runs failed earlier,
so this historical result does not establish a capacity floor.

**Configurable application pool.** Npgsql `Maximum Pool Size`
is now read from `Database:MaxPoolSize` by
`SAPSec.Infrastructure/Postgres/NpgsqlDataSourceFactory.cs`, defaulting to 100.
Terraform defaults to 100 for Test and Review. See the comparison above for run status.
PgBouncer - built into Azure Flexible Server - is a tidier longer-term
option that multiplexes many app connections onto few database ones, letting
the app pool generously without each connection pinning a database backend.

## Expected usage (from analytics)

The analytics team's usage data for the comparable CSCP service:

- Daily usage typically 1,000-5,000 users; exceeds 6,000 only occasionally;
  single all-time peak ~12,000.

Converted to **concurrent** users (the unit the load test measures), using an
assumed mean session length and peak-window concentration:

- Normal day: ~45 concurrent users.
- Busy day: ~220 concurrent users.

The normal-day estimate is within the range that met both targets in the
recent runs. The busy-day estimate is above the first measured latency
breach. Both estimates depend on session length and how concentrated
traffic is.

## Historical operating targets (require reassessment)

These proposals came from the historical production-tier run and are
retained as context. Neither recent run met both targets at 150 users.

| Target | Proposed value | Basis |
|---|---|---|
| Guaranteed load | **150 concurrent users** | p95 under 800ms, zero errors, real headroom. The number to put in an SLA. |
| Expected load | **~45 normal / ~220 busy-day concurrent** | Converted from analytics daily figures (above), not a load-test output. Comfortably inside the tested envelope. |
| Target peak load | **150 (fast bar) or ~850 (working bar)** | Depends on the quality bar. If the bar is p95 < 3s, ~150. If it's "no errors, slower acceptable," ~850 (failures stay at zero to there). Given a realistic busy-day peak of ~220, ~850 is suggested so the target clears expected demand, with 150 as the fast/comfortable mark. Needs a steer on which bar the assessment holds to. |
| Maximum before collapse / shedding | **~1,001 concurrent users** | First real failures appear here, rising to ~47% by 1,400. Degrades gradually rather than shedding cleanly. Well above any realistic demand, and conservative - measured with the connection pool at 100 per replica. |

## Open actions

1. **Investigate the timeouts and HTTP 502s** using ingress, pod, and
   PostgreSQL metrics. Keep the pool default at 100 until the evidence
   supports a different setting.
2. **Confirm the pod restart mechanism and memory limit.** Exit Code 137 at
   ~18:16 suggests a possible OOM kill separate from the earlier liveness
   failures. Confirm with infra whether the pods hit their configured memory
   limit and what it is set to, so the pool change isn't assumed to fix a
   memory problem. More connections use slightly more memory, so if memory is
   tight, review the limit alongside the pool change.
3. **Database `max_connections` confirmed at 856** on the Test tier
   (`sapsec_test`). A pool of ~200-300 per replica gives 400-600, safely
   under 856 with headroom for the maintenance pod and the co-located
   `sap-public` service. Confirm the production tier's value before applying
   there.
4. **Restore normal Test settings after the run.** Redeploy with a pool
   of 100 and the bypass disabled. Verify a signed-out request to
   `/find-a-school` redirects to DfE Sign-in.
5. **Revert the `test` database** to its normal tier if the production-spec
   scaling was temporary.
6. Confirm whether an HPA is configured (pods scaled during the run) with the
   platform team, and reconcile this doc with the main README.
7. Confirm the actual WAF rate-limit window/duration with whoever owns the
   `domains` Terraform module.
8. Firm up the analytics-to-concurrent conversion (session length, peak
   concentration) with the analytics team.
9. Get the four operating target numbers - and the fast-vs-working decision
   for target peak - signed off by the service owner.

## How to run it

See [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for step-by-step setup, and [load_testing/README.md](../../load_testing/README.md)
for the full suite (scenarios, environments, CI workflow).
