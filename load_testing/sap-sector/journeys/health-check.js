import http from 'k6/http'
import { group, check } from 'k6'
import { performanceCheck } from '../utils/checks.js'

export function healthCheckJourney (environment, config) {
  group('SAP Sector: Health Check Journey', function () {
    const basic = http.get(`${environment.baseUrl}/healthcheck`)
    performanceCheck(basic, 'Basic health check', config.expectedResponseTimes.health)
    check(basic, {
      'Basic health check: body is Healthy': (r) => r.body != null && r.body.includes('Healthy')
    })

    const detailed = http.get(`${environment.baseUrl}/health`)
    performanceCheck(detailed, 'Detailed health check', config.expectedResponseTimes.health)
    check(detailed, {
      'Detailed health check: status is Healthy': (r) => {
        try {
          return r.json('status') === 'Healthy'
        } catch {
          return false
        }
      }
    })
  })
}
