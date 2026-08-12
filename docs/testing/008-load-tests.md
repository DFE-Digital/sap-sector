# Load Testing

Load testing verifies the service can handle expected and peak usage, using
[k6](https://k6.io/) - see [load_testing/](../../load_testing/README.md) for
the full test suite and [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for how to run it.

## Status

Draft for service assessment (raised by Robert Rees). Scenarios and journeys
are built and verified locally; the operating targets below are proposed
starting points, not yet signed off or backed by production usage data. See
[Open actions](#open-actions).

## Hosting model

The application runs on Azure Kubernetes Service (AKS):

- Behind Azure Front Door, with WAF rate limiting enabled -
  `rate_limit_max` is **300 req/s on test** and **1000 req/s on production**,
  with `block_ip: true` on repeat offenders
  (`terraform/domains/environment_domains/config/{test,production}.tfvars.json`).
  This is a real, already-in-place traffic-shedding mechanism.
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

**Not yet run**: `baseline`/`peak-surge`/`stress` against a real review or
test deployment. Local results don't reflect the real 2-pod capacity, real
database latency, or the WAF sitting in front of test/production - see
[Running against the test environment](../../load_testing/README.md#running-against-the-test-environment)
for what that needs (team coordination, since it's shared infrastructure).

## Operating targets (proposed - needs sign-off)

Per Robert Rees' service assessment ask, four targets:

| Target | Proposed value | Basis |
|---|---|---|
| Guaranteed load | TBC | Needs real user-base sizing - number of schools in scope × a plausible concurrent-access percentage. Not yet estimated. |
| Expected load | ~10 concurrent users | School leaders checking in during work hours; likely genuinely low given the tool's scope. Matches the `baseline` scenario. |
| Target peak load | ~50 concurrent users | Speculative "surge" event (e.g. results day, start of term). No confirmed trigger event has been identified yet - this is a guess, not a target. |
| Maximum load before collapse / shedding | Bounded by the Front Door WAF: 300 req/s (test) / 1000 req/s (production), then IP-blocked | Real infra ceiling, not app-level. Below that ceiling, the practical constraint is the fixed 2-pod deployment - sustained load there would show as rising latency/5xx well before the WAF limit is reached. |

**Note for the assessment conversation**: given the service's actual
audience (school leaders, business hours, no public/mass-market traffic),
the case for deprioritising "guaranteed load" as a formal target - rather
than engineering to a number nobody can currently justify - seems reasonable
per Robert's own framing. Worth raising directly rather than inventing a
number to fill the gap.

## Open actions

1. Confirm the autoscaling question with the platform/infra team, and
   correct either this doc or the main README (they currently disagree).
2. Size the real user base (schools in scope) to replace "TBC" and the
   other proposed numbers with defensible ones.
3. Run `baseline` and `peak-surge` against `test` (or a review app) and
   record real results here.
4. Get the four operating target numbers - or the decision to deprioritise
   some of them - signed off by the service owner.

## How to run it

See [load_testing/RUNNING_LOCALLY.md](../../load_testing/RUNNING_LOCALLY.md)
for step-by-step setup, and [load_testing/README.md](../../load_testing/README.md)
for the full suite (scenarios, environments, CI workflow).
