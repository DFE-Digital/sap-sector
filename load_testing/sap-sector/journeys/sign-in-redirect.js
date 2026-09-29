import http from 'k6/http'
import { group } from 'k6'
import { performanceCheck } from '../utils/checks.js'

// Only checks that the "Start now" link correctly challenges to DfE Sign-in.
// The actual DSI OpenID Connect flow needs real user credentials and is
// intentionally out of scope for this load test.
export function signInRedirectJourney (environment, config) {
  group('SAP Sector: Sign-in Redirect Journey', function () {
    const response = http.get(`${environment.baseUrl}/auth/signin`, { redirects: 0 })

    performanceCheck(response, 'Sign-in redirect', config.expectedResponseTimes.authRedirect, [301, 302, 303])
  })
}
