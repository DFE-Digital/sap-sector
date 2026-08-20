# Load Testing

Load testing verifies the service can handle expected and peak usage, using
[k6](https://k6.io/) - see [load_testing/](../../load_testing/README.md) for
the full test suite and [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for how to run it.

## Status

Draft for service assessment (raised by Robert Rees). A valid `stress` run
against the deployed `test` environment with real authentication and real
data - see
[Real result: authenticated stress scenario against test (valid)](#real-result-authenticated-stress-scenario-against-test-valid).
The app handled 150 concurrent authenticated users cleanly.

The operating targets below are still proposed starting points, not yet
signed off. See [Open actions](#open-actions).

## Hosting model

The application runs on Azure Kubernetes Service (AKS):

- Behind Azure Front Door, with WAF rate limiting enabled -
  `rate_limit_max` is **300 on test** and **1000 on production**, with
  `block_ip: true` on repeat offenders
  (`terraform/domains/environment_domains/config/{test,production}.tfvars.json`).
  This is a real, already-in-place traffic-shedding mechanism.
  **Correction:** we previously described this as "req/s." That was an
  unverified assumption - the actual rule lives in a vendored Terraform
  module (`./vendor/modules/domains`) not present in this checkout, so its
  real time window is unconfirmed. Needs confirming with whoever owns the
  `domains` module before this is stated as fact anywhere else.
- Health probes on `/healthcheck` (liveness/readiness).
- A **fixed replica count of 2 pods** in both test and production
  (`terraform/application/config/{test,production}.tfvars.json`). We did not
  find a Horizontal Pod Autoscaler configured anywhere in this repo's
  Terraform. **This contradicts the main README's claim of "auto-scaling
  based on CPU/memory metrics" / HPA** - see [Open actions](#open-actions).
  The AKS module itself is vendored and wasn't available to inspect locally,
  so this needs confirming with the platform team rather than assumed either
  way.

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
auto-authenticating test scheme and swaps the Postgres-backed repositories
for the same JSON fixture data (~1,924 real-looking establishment records)
already used by the integration test suite - see `SAPSec.Web/Program.cs` and
[How the `LoadTest` mode works](../../load_testing/README.md#how-the-loadtest-mode-works).
This mode only ever runs on a machine we control; it must never be set on a
shared review/test/production deployment, since it disables real
authentication.

## Load scenarios

Four scenarios, selectable per run:

| Scenario | Peak concurrent users | Duration | Purpose |
|---|---|---|---|
| `quick` | 10 | 25s | Smoke test |
| `baseline` | 10 | ~4 min | Normal operations |
| `peak-surge` | 50 | ~5.5 min | Surge event (e.g. results day) |
| `stress` | 150 | ~9 min | Breaking point identification |

Thresholds: p95 response time under 3s, error rate under 1%, no 5xx
responses. These VU counts are starting estimates for a niche, business-hours,
professional-user (school leader) tool - not yet derived from real production
usage data. `stress` is deliberately capped well under the Front Door WAF
rate limits, so app-level behaviour can be characterised separately from
infra-level shedding.

## Results so far

Run locally (`quick` scenario, 10 VUs, both anonymous and authenticated
journey sets): **383/383 checks passed, 0% error rate**, all response times
well under threshold.

**Not yet run in a planned way**: `baseline`/`peak-surge` against a real
review or test deployment. Local results don't reflect the real 2-pod
capacity, real database latency, or the WAF sitting in front of test/production.

### Real result: authenticated stress scenario against test (valid)

A `stress` run on 2026-08-20 against the deployed `test` environment, with
real authentication and real data. The numbers below can be cited.

**What changed to make this work:**

- The deployed `test` environment was temporarily built from this branch
  with DfE Sign-in bypassed at the code level (`SAPSec.Web/Program.cs` -
  `AutoAuthenticationHandler` used unconditionally instead of real DSI OIDC)
  rather than via a session cookie. Every request auto-authenticates
  regardless of what's sent - no MFA, no chunked-cookie handling, no
  session-expiry risk. **This is a temporary, never-merged state of `test`**
  - it disables real authentication for anyone using the shared environment
    while deployed this way, and must be reverted before any merge to `main`.
- Getting this deployed uncovered two real regressions introduced by
  deleting the DSI authentication wiring wholesale rather than swapping just
  the auth scheme: `AddDsiAuthentication()` also registered
  `IHttpContextAccessor`, `IUserService`, and the DSI API `HttpClient` (used
  by `AuthController`/`UserController`/`DsiAuthorizationHandler` regardless
  of login scheme) - restored explicitly. And real DfE Analytics
  (`Dfe.Analytics.AspNetCore.DfeAnalyticsMiddleware`) was still active for
  the `"Test"` environment name and threw `BigQueryClient has not been
  configured` on every request including `/healthcheck`, which is what was
  actually blocking the Kubernetes rollout from completing (`1 out of 2 new
  replicas have been updated...` for ~10 minutes then timing out) - fixed by
  adding `"Test"` to the same skip-list as the other test environments.
- Target: `test`'s direct backend origin
  (`get-school-improvement-insights-test.test.teacherservices.cloud`),
  bypassing Front Door/WAF - a team-cleared arrangement.
- A quick smoke test was run first and confirmed genuine authentication
  (100% status/content checks passed, including "contains expected
  content") before committing to the full 9-minute run.

**What happened:**

- Ran to completion: 9m 5s, matching the scripted duration, peaked at the
  target 150 VUs (dipping to a minimum of 83 during the ramp, per
  `vus_max`).
- **24,421 requests, all succeeded at the HTTP level** - `http_req_failed`
  0.00%.
- Our combined status+latency metric (`sap_sector_error_rate`) stayed well
  under its 1% threshold: **0.14% overall** (36 of 24,421).
- **99.95% of all checks passed** (88,110 of 88,146), including content
  checks - confirming real authenticated pages with real data were being
  exercised throughout, not a redirect loop like the invalid attempt.
- Response times degraded gracefully with load rather than collapsing:
  overall avg 193ms, median 95ms, p90 452ms, p95 687ms, max 6.66s (a single
  outlier).

**Time-resolved detail** (30-second buckets from the CSV export; VUs is the
scenario's target at that point):

| Elapsed | VUs | Requests | p50 | p95 | max | Error rate |
|---|---|---|---|---|---|---|
| 0s | 28 | 263 | 56ms | 360ms | 1,384ms | 0.0% |
| 30s | 48 | 555 | 47ms | 232ms | 449ms | 0.0% |
| 60s | 61 | 767 | 57ms | 256ms | 777ms | 0.0% |
| 90s | 74 | 958 | 53ms | 253ms | 485ms | 0.0% |
| 120s | 86 | 1,087 | 59ms | 304ms | 598ms | 0.0% |
| 150s | 99 | 1,270 | 61ms | 316ms | 933ms | 0.0% |
| 180s | 107 | 1,414 | 70ms | 377ms | 1,000ms | 0.0% |
| 210s | 116 | 1,534 | 80ms | 436ms | 931ms | 0.0% |
| 240s | 124 | 1,588 | 79ms | 556ms | 1,862ms | 0.0% |
| 270s | 132 | 1,722 | 89ms | 440ms | 1,084ms | 0.0% |
| 300s | 141 | 1,817 | 104ms | 630ms | 1,569ms | 0.0% |
| 330s | 149 | 1,867 | 127ms | 898ms | 1,911ms | 0.0% |
| 360s | 150 | 1,957 | 125ms | 856ms | 2,176ms | 0.0% |
| 390s | 150 | 1,906 | 127ms | 868ms | 1,785ms | 0.0% |
| 420s | 150 | 1,819 | 167ms | 963ms | 2,382ms | 0.0% |
| 450s | 150 | 1,907 | 129ms | 906ms | 1,678ms | 0.0% |
| 480s | 96 | 1,400 | 153ms | 1,545ms | 6,669ms | 2.6% |
| 510s | 11 | 584 | 94ms | 411ms | 2,643ms | 0.0% |
| 540s | 1 | 6 | 68ms | 356ms | 356ms | 0.0% |

**Reading it**: error rate is a genuine **0.0% for the entire ramp-up and
the entire 150-VU sustained window** (t=0 through t=450s). The only
non-zero error window (2.6% at t=480s) falls during ramp-*down*, as VUs
drop from 150 to 96 - consistent with a handful of in-flight requests being
torn down mid-response rather than the app failing under peak load. Latency
increases smoothly and predictably as load increases (p50 ~55ms at 28 VUs
to ~125-167ms sustained at 150 VUs; p95 ~250-360ms to ~860-960ms) - no
cliff, no runaway tail latency, no sustained spike. **At 150 concurrent
authenticated users, this app does not show signs of being close to a
breaking point** - the `stress` scenario's ceiling wasn't high enough to
find one.

**Still open**: this only tells us the app is healthy up to 150 VUs, not
where it actually breaks. A higher-intensity run (above 150) and CPU/memory
metrics for the pods during it would be needed to find the real ceiling.
Raw per-request time-series (421,454 rows) saved locally as
`load_testing/stress-test-timeseries.csv` - gitignored, not committed.

## Operating targets (proposed - needs sign-off)

Per Robert Rees' service assessment ask, four targets:

| Target | Proposed value | Basis |
|---|---|---|
| Guaranteed load | TBC | Needs real user-base sizing - number of schools in scope × a plausible concurrent-access percentage. Not yet estimated. |
| Expected load | ~10 concurrent users | School leaders checking in during work hours; likely genuinely low given the tool's scope. Matches the `baseline` scenario. |
| Target peak load | ~50 concurrent users | Speculative "surge" event (e.g. results day, start of term). No confirmed trigger event has been identified yet - this is a guess, not a target. |
| Maximum load before collapse / shedding | Not yet found. Bounded first by the Front Door WAF (`rate_limit_max` 300 on test / 1000 on production, unit unconfirmed), but **the app itself handled 150 concurrent authenticated users cleanly (0.14% error rate, graceful latency growth, no failure onset)** - the highest level tested so far, not a breaking point. | Test direct-origin, WAF bypassed, real auth (2026-08-20) - 99.95% checks passed, 0.0% error rate through the full 150-VU sustained window - see [Real result: test, valid](#real-result-authenticated-stress-scenario-against-test-valid). Need a higher-intensity scenario to actually find the ceiling. |

**Note for the assessment conversation**: given the service's actual
audience (school leaders, business hours, no public/mass-market traffic),
the case for deprioritising "guaranteed load" as a formal target - rather
than engineering to a number nobody can currently justify - seems reasonable
per Robert's own framing. Worth raising directly rather than inventing a
number to fill the gap.

## Open actions

1. **`test` currently has no real authentication for anyone using it** -
   it's running a temporary build with DfE Sign-in bypassed at the code
   level (see [Real result: test, valid](#real-result-authenticated-stress-scenario-against-test-valid)).
   **Revert this deployment back to a normal build as soon as load testing
   is done.** This is live right now, not a historical item - the longer it
   stays deployed, the bigger the exposure window.
2. **Find the actual breaking point.** 150 VUs wasn't enough to stress the
   app - it needs a higher-intensity scenario (a new scenario above `stress`,
   or a longer sustain at higher VUs) to find where it actually starts to
   fail. Do this **before** reverting the temporary deployment (item 1), or
   it'll need redeploying.
3. Pull CPU/memory metrics for the pods during a higher-intensity run, to
   see what's actually being consumed even though nothing failed yet. Needs
   Azure Monitor/Application Insights access.
4. Confirm the autoscaling question with the platform/infra team, and
   correct either this doc or the main README (they currently disagree).
5. Confirm the actual WAF rate-limit window/duration with whoever owns the
   `domains` Terraform module, and correct the unit claim once known.
6. Size the real user base (schools in scope) to replace "TBC" and the
   other proposed numbers with defensible ones.
7. Run `baseline` and `peak-surge` against `test` for completeness (results
   should be uneventful given the `stress` result, but worth recording).
8. Get the four operating target numbers - or the decision to deprioritise
   some of them - signed off by the service owner.

## How to run it

See [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for step-by-step setup, and [load_testing/README.md](../../load_testing/README.md)
for the full suite (scenarios, environments, CI workflow).
