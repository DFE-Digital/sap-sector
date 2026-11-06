import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck } from '../utils/checks.js'
import { authParams } from '../utils/auth.js'
import { randomSecondaryUrn } from '../data/school-urns.js'

// The standalone /ComparePerformance page no longer exists (it has no view), so
// this journey now exercises the similar-schools comparison list instead.
// Requires DfE Sign-in. Either run against an app instance started with
// ASPNETCORE_ENVIRONMENT=LoadTest (see load_testing/README.md), or against a
// real environment with a SESSION_COOKIE set - see utils/auth.js.
export function comparePerformanceJourney (environment, config) {
  group('SAP Sector: Compare Performance Journey', function () {
    const urn = randomSecondaryUrn()
    const response = http.get(`${environment.baseUrl}/school/secondary/${urn}/view-similar-schools`, authParams())

    performanceCheck(response, 'Compare performance (similar schools)', config.expectedResponseTimes.schoolPage)

    sleep(1)
  })
}
