import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js'
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js'

import { sleep, group } from 'k6'
import { getEnvironment, getConfig } from './config/environment.js'
import { homepageJourney } from './journeys/homepage.js'
import { staticPageJourney } from './journeys/static-pages.js'
import { healthCheckJourney } from './journeys/health-check.js'
import { signInRedirectJourney } from './journeys/sign-in-redirect.js'
import { schoolSearchJourney } from './journeys/school-search.js'
import { secondarySchoolJourney, primarySchoolJourney } from './journeys/school-overview.js'
import { comparePerformanceJourney } from './journeys/compare-performance.js'
import { hasSessionCookie } from './utils/auth.js'
import { quickTestScenario } from './scenarios/quick-test.js'
import { baselineScenario } from './scenarios/baseline.js'
import { peakSurgeScenario } from './scenarios/peak-surge.js'
import { stressTestScenario, buildStepThresholds, applyStepTag } from './scenarios/stress-test.js'
import { randomThinkTime } from '../shared/utils/common-helpers.js'

const SCENARIO = __ENV.SCENARIO || 'quick'

function getSelectedScenario () {
  switch (SCENARIO) {
    case 'baseline':
      return { sap_sector_baseline: baselineScenario }
    case 'peak-surge':
      return { sap_sector_peak_surge: peakSurgeScenario }
    case 'stress':
      return { sap_sector_stress: stressTestScenario }
    case 'quick':
      return { sap_sector_quick: quickTestScenario }
    default:
      return { sap_sector_quick: quickTestScenario }
  }
}

// Per-step thresholds only apply to the stress staircase - they are what
// materialise the per-load-level sub-metrics in the summary JSON. Adding them
// to other scenarios would declare thresholds against tags never emitted.
function getThresholds () {
  const base = getConfig().thresholds
  if (SCENARIO !== 'stress') return base
  return Object.assign({}, base, buildStepThresholds(3000))
}

function resolveInsecureSkipTLSVerify () {
  try {
    return getEnvironment().insecureSkipTLSVerify || false
  } catch {
    return false
  }
}

export const options = {
  scenarios: getSelectedScenario(),
  thresholds: getThresholds(),
  insecureSkipTLSVerify: resolveInsecureSkipTLSVerify(),
  tags: {
    service: 'sap-sector',
    testType: 'load'
  }
}

export function setup () {
  const environment = getEnvironment()
  const config = getConfig()
  console.log(`Testing ${config.service} - ${environment.name}: ${environment.baseUrl}`)
  return { environment, config }
}

export default function (data) {
  const { environment, config } = data

  // Stamps every metric this VU emits for the rest of the iteration with the
  // staircase step it belongs to. One line here covers all journeys.
  if (SCENARIO === 'stress') {
    applyStepTag()
  }

  const journeyChoice = Math.random()

  // School search/comparison sits behind DfE Sign-in (OpenID Connect) and
  // can't be scripted against a real deployment without user credentials.
  // The authenticated journeys run in three cases:
  //   - "loadtest" environment: an instance you run yourself with
  //     ASPNETCORE_ENVIRONMENT=LoadTest (see README.md), auth bypassed.
  //   - Any environment when SESSION_COOKIE is set: a real session obtained
  //     by manually signing in once (through MFA) as a dedicated test/service
  //     account and replaying that cookie - see utils/auth.js.
  //   - Any environment when AUTH_OFF=true: for an instance where sign-in has
  //     been switched off at the deployment (e.g. an isolated test env). No
  //     session is involved; the real service journeys just run directly.
  // Every other case sticks to the anonymous-only mix.
  if (environment.name === 'loadtest' || hasSessionCookie() || __ENV.AUTH_OFF === 'true') {
    group('SAP Sector Authenticated User Journey', function () {
      if (journeyChoice < 0.4) {
        // 40% - search for a school
        schoolSearchJourney(environment, config)
      } else if (journeyChoice < 0.7) {
        // 30% - view a secondary school's performance pages
        secondarySchoolJourney(environment, config)
      } else if (journeyChoice < 0.9) {
        // 20% - view a primary school's performance pages
        primarySchoolJourney(environment, config)
      } else {
        // 10% - compare performance page
        comparePerformanceJourney(environment, config)
      }
    })
  } else {
    group('SAP Sector User Journey', function () {
      if (journeyChoice < 0.55) {
        // 55% - land on the homepage
        homepageJourney(environment, config)
      } else if (journeyChoice < 0.85) {
        // 30% - browse footer/static content pages
        staticPageJourney(environment, config)
      } else if (journeyChoice < 0.95) {
        // 10% - click "Start now" and hit the DfE Sign-in redirect boundary
        group('Full SAP Sector Journey', function () {
          homepageJourney(environment, config)
          signInRedirectJourney(environment, config)
        })
      } else {
        // 5% - simulate a monitoring/liveness probe
        healthCheckJourney(environment, config)
      }
    })
  }

  // Think time between actions
  sleep(randomThinkTime(1, 4))
}

export function handleSummary (data) {
  return {
    'sap-sector-load-test-summary.json': JSON.stringify(data, null, 2),
    'sap-sector-load-test-report.html': htmlReport(data, {
      title: 'SAP Sector Load Test Report'
    }),
    stdout: textSummary(data, { indent: ' ', enableColors: true })
  }
}