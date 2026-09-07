import exec from 'k6/execution'

// ---------------------------------------------------------------------------
// Staircase definition.
//
// The stages below are GENERATED from VU_STEPS so that the step boundaries and
// the tag boundaries can never drift apart. Do not hand-edit the stages array -
// edit VU_STEPS, RAMP_SECONDS or HOLD_SECONDS and everything else follows.
//
// Each level ramps for RAMP_SECONDS then holds for HOLD_SECONDS. Only the hold
// is steady state, so only the hold is tagged with a step id; traffic during a
// ramp is tagged 'ramp' and excluded from the per-step figures. Without that
// separation a p95 cannot be attributed to any particular load level.
// ---------------------------------------------------------------------------

const VU_STEPS = [50, 100, 150, 250, 450, 650, 850, 1001, 1400, 1800, 2400]

const RAMP_SECONDS = 30
const HOLD_SECONDS = 90
const STEP_SECONDS = RAMP_SECONDS + HOLD_SECONDS

// After the staircase, drop back to a low level to observe recovery. Whether
// the service returns to baseline latency after being saturated is separate
// evidence from how it behaved while saturated.
const RECOVERY_VUS = 150
const RECOVERY_SECONDS = 120
const RAMPDOWN_SECONDS = 60

export const stressTestScenario = {
  executor: 'ramping-vus',
  startVUs: 10,
  stages: buildStages(),
  gracefulRampDown: '30s',
  tags: {
    service: 'sap-sector',
    scenario: 'stress',
    description: `Stress test - staircase to ${VU_STEPS[VU_STEPS.length - 1]} VUs, ${HOLD_SECONDS}s hold at each level`
  }
}

function buildStages () {
  const stages = []
  for (const target of VU_STEPS) {
    stages.push({ duration: `${RAMP_SECONDS}s`, target })
    stages.push({ duration: `${HOLD_SECONDS}s`, target })
  }
  stages.push({ duration: `${RAMP_SECONDS}s`, target: RECOVERY_VUS })
  stages.push({ duration: `${RECOVERY_SECONDS}s`, target: RECOVERY_VUS })
  stages.push({ duration: `${RAMPDOWN_SECONDS}s`, target: 0 })
  return stages
}

// ---------------------------------------------------------------------------
// Step identity.
//
// Zero-padded so the tags sort correctly as strings in the summary and in any
// spreadsheet the numbers end up in: vu0050, vu0100 ... vu2400.
// ---------------------------------------------------------------------------

function stepId (vus) {
  return `vu${String(vus).padStart(4, '0')}`
}

export function currentStepId () {
  const elapsed = exec.instance.currentTestRunDuration / 1000
  const index = Math.floor(elapsed / STEP_SECONDS)

  if (index >= VU_STEPS.length) return 'recovery'
  if (elapsed % STEP_SECONDS < RAMP_SECONDS) return 'ramp'
  return stepId(VU_STEPS[index])
}

// ---------------------------------------------------------------------------
// Tagging.
//
// exec.vu.tags applies to every metric the VU emits for the rest of the
// iteration, so one call at the top of the default function covers all the
// journeys without touching a single journey file. An iteration that straddles
// a boundary keeps the step it started in, which is the correct attribution.
//
// If your k6 is too old for exec.vu.tags this silently does nothing rather
// than failing the run - check `k6 version` and see the fallback in the notes.
// ---------------------------------------------------------------------------

export function applyStepTag () {
  try {
    exec.vu.tags.step = currentStepId()
  } catch (err) {
    // exec.vu.tags unavailable on this k6 version - per-step sub-metrics will
    // be empty, the run itself is unaffected.
  }
}

// ---------------------------------------------------------------------------
// Threshold sub-metrics.
//
// k6 materialises a sub-metric for any tag selector a threshold names, and
// includes its full trend statistics in the handleSummary JSON. Declaring one
// threshold per step is therefore what puts the entire staircase - p95 at every
// load level - into a ~50 KB JSON file, and is why the CSV stops being the
// primary evidence.
//
// The per-step thresholds are informational: abortOnFail is false because a
// step breaching 3s is the finding, not a reason to stop. The global
// http_req_failed threshold does abort, because once requests are genuinely
// failing there is nothing further to learn and a shared environment should
// not be held under load unnecessarily.
// ---------------------------------------------------------------------------

export function buildStepThresholds (sloMs = 3000) {
  const thresholds = {}

  for (const target of VU_STEPS) {
    const selector = `{step:${stepId(target)}}`
    thresholds[`http_req_duration${selector}`] = [`p(95)<${sloMs}`]
    thresholds[`http_req_failed${selector}`] = ['rate<0.01']
  }

  thresholds['http_req_duration{step:recovery}'] = [`p(95)<${sloMs}`]

  thresholds.http_req_failed = [
    { threshold: 'rate<0.05', abortOnFail: true, delayAbortEval: '30s' }
  ]
  thresholds.dropped_iterations = ['count<1000']

  return thresholds
}

export function stepPlan () {
  return {
    steps: VU_STEPS,
    rampSeconds: RAMP_SECONDS,
    holdSeconds: HOLD_SECONDS,
    totalSeconds: VU_STEPS.length * STEP_SECONDS + RAMP_SECONDS + RECOVERY_SECONDS + RAMPDOWN_SECONDS
  }
}