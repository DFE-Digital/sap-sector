import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'

export function homepageJourney (environment, config) {
  group('SAP Sector: Homepage Journey', function () {
    const response = http.get(`${environment.baseUrl}/`)

    performanceCheck(response, 'Homepage', config.expectedResponseTimes.homepage)
    contentCheck(response, 'Get school improvement insights', 'Homepage heading')

    sleep(1)
  })
}
