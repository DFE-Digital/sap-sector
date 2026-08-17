# Load Testing

Load testing verifies the service can handle expected and peak usage, using
[k6](https://k6.io/) - see [load_testing/](../../load_testing/README.md) for
the full test suite and [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for how to run it.

## Status

Draft for service assessment (raised by Robert Rees). Scenarios and journeys
are built and verified locally, plus one real (unplanned) production result
under stress - see
[Real result: stress scenario against production](#real-result-stress-scenario-against-production-unplanned).
The operating targets below are still proposed starting points, not yet
signed off. See [Open actions](#open-actions).

## Hosting model

The application runs on Azure Kubernetes Service (AKS):

- Behind Azure Front Door, with WAF rate limiting enabled -
  `rate_limit_max` is **300 on test** and **1000 on production**, with
  `block_ip: true` on repeat offenders
  (`terraform/domains/environment_domains/config/{test,production}.tfvars.json`).
  This is a real, already-in-place traffic-shedding mechanism, confirmed by a
  real (unplanned) production run - see
  [Real result: stress scenario against production](#real-result-stress-scenario-against-production-unplanned).
  **Correction:** we previously described this as "req/s." That was an
  unverified assumption - the actual rule lives in a vendored Terraform
  module (`./vendor/modules/domains`) not present in this checkout, so its
  real time window is unconfirmed. The production run below sustained only
  ~34 achieved req/s on average yet was still heavily shed, which is hard to
  reconcile with a literal 1000-per-second reading - the real window is very
  likely longer than one second (e.g. per-minute). Needs confirming with
  whoever owns the `domains` module before this is stated as fact anywhere
  else.
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

### Real result: stress scenario against production (unplanned)

An unplanned `stress` run was executed directly against production
(`https://get-school-improvement-insights.education.gov.uk`) on 2026-08-12,
outside the coordination process this doc recommends (see
[Running against the test environment](../../load_testing/README.md#running-against-the-test-environment)).
It's included here because the result is genuinely useful evidence for the
"maximum load before collapse or traffic shedding" question, and because
transparency about how it was produced matters for how it should be read.

**Provenance note**: the run's recorded `environment.name` is `"local"` even
though the target was production - it was fired via a `LOCAL_URL` override
rather than the built-in `production` environment, so it bypassed the
intentional guardrail of production not being wired to an npm script. This
is a process gap worth closing (e.g. making the tooling refuse to run
against a real host unless `ENVIRONMENT=production` is explicit), not just a
one-off mistake.

**What happened:**

- The scenario ran to completion (not manually stopped) - measured duration
  540.8s (~9.0 min), matching the `stress` script's full ramp exactly, and
  peaked at the target 150 VUs.
- **83.08% of all HTTP requests failed** (15,247 of 18,353). Both the
  `http_req_failed` and our custom `sap_sector_error_rate` thresholds
  (rate < 1%) failed - k6 itself reported the run as failed.
- The failures were rejections, not application errors: every "no server
  errors (5xx)" check passed across every journey (429 is a 4xx), while
  "status is expected" (200) failed 80-84% of the time. The service did not
  error - requests were turned away before reaching it.
- When a request did get through, it was fast: p95 31.4ms overall (p95
  41.6ms restricted to successful requests only), max 355ms. This is a
  reasonably strong signal that the application pods themselves were never
  actually stressed - the WAF appears to have absorbed the excess load
  before it reached the app.
- The failure rate was **uniform across every journey tested** - health
  checks (18.4-18.5% success), homepage (17.2% / 16.2% in the two branches
  that exercise it), static pages (16.3-16.8%), sign-in redirect (15.9%) all
  land in the same narrow 16-18.5% band. That points to broad (likely
  per-IP or per-connection) rate limiting rather than anything route-specific.

| Metric | Value |
|---|---|
| Total requests | 18,353 |
| Requests failed | 15,247 (83.08%) |
| Checks passed | 42,655 / 71,755 (59.45%) |
| Peak VUs | 150 |
| Test duration | 540.8s (full scripted run) |
| p95 response time (successful requests) | 41.6ms |
| `http_req_duration` p95 < 3000ms threshold | Passed |
| `http_req_failed` rate < 1% threshold | **Failed** |

**What this data can't tell us** (aggregate summary only, no per-request
timestamps):

- The exact elapsed-time onset of shedding - can't say "it started failing
  at minute N." A future run would need `k6 run --out csv=results.csv` (or
  similar time-series output) instead of just the default summary to answer
  that precisely.
- The real WAF rate-limit window/duration - see the correction under
  [Hosting model](#hosting-model).
- App-level metrics (CPU/memory on the 2 production pods) during the run -
  would confirm whether the app was genuinely shielded by the WAF or partly
  stressed itself. Needs Azure Monitor/Application Insights access, which
  wasn't available when writing this up.

## Operating targets (proposed - needs sign-off)

Per Robert Rees' service assessment ask, four targets:

| Target | Proposed value | Basis |
|---|---|---|
| Guaranteed load | TBC | Needs real user-base sizing - number of schools in scope × a plausible concurrent-access percentage. Not yet estimated. |
| Expected load | ~10 concurrent users | School leaders checking in during work hours; likely genuinely low given the tool's scope. Matches the `baseline` scenario. |
| Target peak load | ~50 concurrent users | Speculative "surge" event (e.g. results day, start of term). No confirmed trigger event has been identified yet - this is a guess, not a target. |
| Maximum load before collapse / shedding | Bounded by the Front Door WAF (`rate_limit_max` 300 on test / 1000 on production, unit unconfirmed - see [Hosting model](#hosting-model)), then IP-blocked | Real infra ceiling, not app-level, and now backed by an actual (if unplanned) production result: under a 150-VU stress run, 83.08% of requests were shed at the edge while the ~17% that got through stayed fast (p95 41.6ms) - see [Real result](#real-result-stress-scenario-against-production-unplanned). The app itself was likely never stressed; the WAF did the shedding. |

**Note for the assessment conversation**: given the service's actual
audience (school leaders, business hours, no public/mass-market traffic),
the case for deprioritising "guaranteed load" as a formal target - rather
than engineering to a number nobody can currently justify - seems reasonable
per Robert's own framing. Worth raising directly rather than inventing a
number to fill the gap.

## Open actions

1. Confirm the autoscaling question with the platform/infra team, and
   correct either this doc or the main README (they currently disagree).
2. Confirm the actual WAF rate-limit window/duration with whoever owns the
   `domains` Terraform module, and correct the unit claim once known.
3. Size the real user base (schools in scope) to replace "TBC" and the
   other proposed numbers with defensible ones.
4. Run `baseline` and `peak-surge` against `test` (or a review app), through
   the intended coordinated process this time, and record real results here.
5. Re-run `stress` with a time-series output (`--out csv=...`) to pin down
   exactly when shedding starts, ideally against `test` rather than
   production.
6. Pull CPU/memory metrics for the 2 production pods during the 2026-08-12
   run, if still available in Azure Monitor, to confirm the app was shielded
   rather than partly stressed.
7. Get the four operating target numbers - or the decision to deprioritise
   some of them - signed off by the service owner.
8. Decide and close the process gap that let a `stress` run reach production
   unlabelled and uncoordinated (see the provenance note under
   [Real result](#real-result-stress-scenario-against-production-unplanned)).

## How to run it

See [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for step-by-step setup, and [load_testing/README.md](../../load_testing/README.md)
for the full suite (scenarios, environments, CI workflow).
