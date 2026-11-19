import exec from 'k6/execution'

// ---------------------------------------------------------------------------
// School-journey capacity staircase.
//
// Ramps the number of VUs through VU_STEPS, holding at each level so the
// figures for that level are steady-state. With think time removed, each VU
// keeps one request in flight continuously, so a step's VU count is the
// number of concurrent requests offered to the app at that level.
//
// Override the defaults without editing this file:
//   --env CAPACITY_STEPS=10,25,50,100   (comma-separated VU levels)
//   --env RAMP_SECONDS=30
//   --env HOLD_SECONDS=120
// ---------------------------------------------------------------------------

const DEFAULT_STEPS = [10, 25, 50, 100, 150, 200, 300]

export const SCHOOL_TYPES = ['secondary', 'primary', 'all-through']

function parseSteps (value) {
  if (!value) return DEFAULT_STEPS
  const steps = value.split(',').map((s) => Number(s.trim()))
  if (!steps.length || steps.some((s) => !Number.isInteger(s) || s <= 0)) {
    throw new Error(`CAPACITY_STEPS must be a comma-separated list of positive integers, got "${value}"`)
  }
  return steps
}

function parseSeconds (value, fallback, name) {
  if (value === undefined || value === '') return fallback
  const seconds = Number(value)
  if (!Number.isInteger(seconds) || seconds <= 0) {
    throw new Error(`${name} must be a positive whole number of seconds, got "${value}"`)
  }
  return seconds
}

const VU_STEPS = parseSteps(__ENV.CAPACITY_STEPS)
const RAMP_SECONDS = parseSeconds(__ENV.RAMP_SECONDS, 30, 'RAMP_SECONDS')
const HOLD_SECONDS = parseSeconds(__ENV.HOLD_SECONDS, 120, 'HOLD_SECONDS')
const STEP_SECONDS = RAMP_SECONDS + HOLD_SECONDS
const RAMPDOWN_SECONDS = 30

export const schoolCapacityScenario = {
  executor: 'ramping-vus',
  startVUs: 0,
  stages: buildStages(),
  gracefulRampDown: '30s',
  tags: {
    service: 'sap-sector',
    scenario: 'school-capacity'
  }
}

function buildStages () {
  const stages = []
  for (const target of VU_STEPS) {
    stages.push({ duration: `${RAMP_SECONDS}s`, target })
    stages.push({ duration: `${HOLD_SECONDS}s`, target })
  }
  stages.push({ duration: `${RAMPDOWN_SECONDS}s`, target: 0 })
  return stages
}

// Zero-padded so tags sort as strings: vu0010, vu0025 ... vu0300. Same format
// as the stress staircase, so analyse.cjs reads both.
function stepId (vus) {
  return `vu${String(vus).padStart(4, '0')}`
}

// Measured from the scenario's own start, so time spent in setup() (the
// warm-up) doesn't shift the step boundaries.
function currentStepId () {
  const elapsed = (Date.now() - exec.scenario.startTime) / 1000
  const index = Math.floor(elapsed / STEP_SECONDS)

  if (index >= VU_STEPS.length) return 'rampdown'
  if (elapsed % STEP_SECONDS < RAMP_SECONDS) return 'ramp'
  return stepId(VU_STEPS[index])
}

// Tags every metric this VU emits for the rest of the iteration (requests,
// checks and custom metrics) with the step and the school type being tested.
// An iteration that straddles a boundary keeps the step it started in.
export function tagIteration (schoolType) {
  exec.vu.tags.step = currentStepId()
  exec.vu.tags.schoolType = schoolType
}

// k6 only reports a tagged sub-metric if a threshold names it, so these
// thresholds are what put the per-step (and per-step-per-school-type) figures
// into the summary JSON. They are informational - a step breaching is the
// finding, not a reason to stop. The overall failure-rate threshold is the
// safety stop: once more than 5% of requests are failing, abort the run.
export function buildCapacityThresholds (sloMs = 3000) {
  const thresholds = {
    http_req_failed: [{ threshold: 'rate<0.05', abortOnFail: true, delayAbortEval: '30s' }]
  }

  for (const target of VU_STEPS) {
    const step = stepId(target)
    thresholds[`http_req_duration{step:${step}}`] = [`p(95)<${sloMs}`]
    thresholds[`http_req_failed{step:${step}}`] = ['rate<0.01']
    thresholds[`sap_sector_content_failed{step:${step}}`] = ['rate<0.01']

    for (const type of SCHOOL_TYPES) {
      thresholds[`http_req_duration{schoolType:${type},step:${step}}`] = [`p(95)<${sloMs}`]
      thresholds[`http_req_failed{schoolType:${type},step:${step}}`] = ['rate<0.01']
    }
  }

  return thresholds
}

export function capacityPlan () {
  return {
    steps: VU_STEPS,
    rampSeconds: RAMP_SECONDS,
    holdSeconds: HOLD_SECONDS,
    totalSeconds: VU_STEPS.length * STEP_SECONDS + RAMPDOWN_SECONDS
  }
}
