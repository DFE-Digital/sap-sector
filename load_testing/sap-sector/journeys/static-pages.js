import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'

const pages = [
  { path: '/accessibility', name: 'Accessibility statement', expectedContent: 'Accessibility statement' },
  { path: '/cookies', name: 'Cookies', expectedContent: 'Cookies' },
  { path: '/terms-and-conditions', name: 'Terms and conditions', expectedContent: 'Terms and conditions' }
]

export function staticPageJourney (environment, config) {
  group('SAP Sector: Static Page Journey', function () {
    const page = pages[Math.floor(Math.random() * pages.length)]
    const response = http.get(`${environment.baseUrl}${page.path}`)

    performanceCheck(response, page.name, config.expectedResponseTimes.staticPage)
    contentCheck(response, page.expectedContent, page.name)

    sleep(1)
  })
}
