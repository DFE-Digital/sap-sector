// School-journey capacity test.
//
// Finds the concurrency level at which the school performance pages stop
// meeting the SLO (p95 < 3s, < 1% failures, correct content). Run it once with
// caching off and once with caching on, against the same app and database, and
// compare the last passing level. See load_testing/README.md
// ("School-journey capacity test") for how to run and read it.

import http from 'k6/http'
import exec from 'k6/execution'
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js'
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js'

import { getEnvironment, getConfig } from './config/environment.js'
import {
  secondarySchoolJourney,
  primarySchoolJourney,
  allThroughSchoolJourney,
  secondarySubPages,
  primarySubPages,
  allThroughSubPages
} from './journeys/school-overview.js'
import { secondarySchoolUrns, primarySchoolUrns, allThroughSchoolUrns } from './data/school-urns.js'
import { schoolCapacityScenario, buildCapacityThresholds, tagIteration, capacityPlan, SCHOOL_TYPES } from './scenarios/school-capacity.js'

const SLO_MS = 3000

const JOURNEYS = {
  secondary: secondarySchoolJourney,
  primary: primarySchoolJourney,
  'all-through': allThroughSchoolJourney
}

const WARMUP_PAGES = {
  secondary: { urns: secondarySchoolUrns, subPages: secondarySubPages },
  primary: { urns: primarySchoolUrns, subPages: primarySubPages },
  'all-through': { urns: allThroughSchoolUrns, subPages: allThroughSubPages }
}

// Only 200 counts as success. Redirects are not followed, so an expired
// session or missing auth shows up as failures instead of quietly loading
// the DfE Sign-in page (and putting load on it).
http.setResponseCallback(http.expectedStatuses(200))

function resolveInsecureSkipTLSVerify () {
  try {
    return getEnvironment().insecureSkipTLSVerify || false
  } catch {
    return false
  }
}

export const options = {
  scenarios: { sap_sector_school_capacity: schoolCapacityScenario },
  thresholds: buildCapacityThresholds(SLO_MS),
  insecureSkipTLSVerify: resolveInsecureSkipTLSVerify(),
  maxRedirects: 0,
  setupTimeout: '10m',
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  tags: {
    service: 'sap-sector',
    testType: 'capacity'
  }
}

// Requests every school's overview and every sub-page once. This warms the
// app (and the cache, when it is on) and verifies that every URN in the pool
// works against this database before any load is applied. Run it for both the
// cache-off and cache-on runs so they start from the same state. Skip it with
// WARMUP=false only when you deliberately want a cold-cache run.
function warmUp (environment) {
  const failures = []
  let requests = 0

  for (const type of SCHOOL_TYPES) {
    const { urns, subPages } = WARMUP_PAGES[type]
    for (const urn of urns) {
      const base = `${environment.baseUrl}/school/${type}/${urn}`
      const urls = [base, ...subPages.map((page) => `${base}/${page}`)]
      const params = { tags: { phase: 'warmup' } }
      const responses = http.batch(urls.map((url) => ['GET', url, null, params]))

      responses.forEach((response, i) => {
        requests++
        const overviewMissingContent = i === 0 && !(response.body && response.body.includes('app-overview'))
        if (response.status !== 200 || overviewMissingContent) {
          failures.push(`${urls[i]} -> ${response.status}${overviewMissingContent && response.status === 200 ? ' (missing overview content)' : ''}`)
        }
      })
    }
  }

  if (failures.length) {
    throw new Error(
      `Warm-up failed for ${failures.length} of ${requests} pages. Fix or remove these URNs in ` +
      `data/school-urns.js (or fix auth) before running the capacity test:\n  ${failures.join('\n  ')}`
    )
  }

  console.log(`Warm-up complete: ${requests} pages returned 200.`)
}

export function setup () {
  const environment = getEnvironment()
  const config = getConfig()

  const plan = capacityPlan()
  console.log(
    `School capacity test - ${environment.name}: ${environment.baseUrl}\n` +
    `  Run label: ${runLabel()}\n` +
    `  VU steps: ${plan.steps.join(', ')} (ramp ${plan.rampSeconds}s, hold ${plan.holdSeconds}s)\n` +
    `  Approx. duration: ${Math.ceil(plan.totalSeconds / 60)} min plus warm-up`
  )

  if (__ENV.WARMUP === 'false') {
    console.log('Warm-up skipped (WARMUP=false): this is a cold-start run.')
  } else {
    warmUp(environment)
  }

  return { environment, config }
}

export default function (data) {
  const { environment, config } = data

  // Round-robin across the run (not random), so every run - cache off or on -
  // gets exactly the same one-third split of secondary, primary and
  // all-through journeys. URNs and sub-pages are still picked at random from
  // each type's pool by the journey itself.
  const schoolType = SCHOOL_TYPES[exec.scenario.iterationInTest % SCHOOL_TYPES.length]
  tagIteration(schoolType)

  JOURNEYS[schoolType](environment, config, { thinkTime: false })
}

function runLabel () {
  return (__ENV.RUN_LABEL || 'run').replace(/[^A-Za-z0-9_-]/g, '-')
}

export function handleSummary (data) {
  const prefix = `school-capacity-${runLabel()}`
  // Recorded alongside the metrics so analyse.cjs can work out requests/sec
  // per step, and so each summary says which run it came from.
  const summary = Object.assign({}, data, {
    capacityPlan: Object.assign(capacityPlan(), {
      runLabel: runLabel(),
      environment: __ENV.ENVIRONMENT || 'local',
      warmUp: __ENV.WARMUP !== 'false'
    })
  })
  return {
    [`${prefix}-summary.json`]: JSON.stringify(summary, null, 2),
    [`${prefix}-report.html`]: htmlReport(data, { title: `SAP Sector School Capacity - ${runLabel()}` }),
    stdout: textSummary(data, { indent: ' ', enableColors: true })
  }
}
