import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'
import { authParams } from '../utils/auth.js'

// Requires DfE Sign-in. Either run against an app instance started with
// ASPNETCORE_ENVIRONMENT=LoadTest (see load_testing/README.md), or against a
// real environment with a SESSION_COOKIE set - see utils/auth.js.
export function comparePerformanceJourney (environment, config) {
  group('SAP Sector: Compare Performance Journey', function () {
    const response = http.get(`${environment.baseUrl}/ComparePerformance`, authParams())

    performanceCheck(response, 'Compare performance', config.expectedResponseTimes.schoolPage)
    contentCheck(response, 'Compare performance', 'Compare performance page')

    sleep(1)
  })
}
