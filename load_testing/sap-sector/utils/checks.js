import { check } from 'k6'
import { Trend, Rate } from 'k6/metrics'

const responseTimeTrend = new Trend('sap_sector_response_time_trend')
const errorRate = new Rate('sap_sector_error_rate')

export function performanceCheck (response, name, threshold = 3000, acceptableStatuses = [200]) {
  responseTimeTrend.add(response.timings.duration)

  const isSuccess = acceptableStatuses.includes(response.status) && response.timings.duration < threshold
  errorRate.add(!isSuccess)

  check(response, {
    [`${name}: status is expected`]: (r) => acceptableStatuses.includes(r.status),
    [`${name}: response time < ${threshold}ms`]: (r) => r.timings.duration < threshold,
    [`${name}: no server errors (5xx)`]: (r) => r.status < 500
  }, {
    endpoint: name,
    service: 'sap-sector'
  })

  if (response.status >= 400) {
    console.error(`SAP Sector Error [${name}]: status=${response.status} url=${response.url} duration=${response.timings.duration}ms`)
  }

  return isSuccess
}

export function contentCheck (response, expectedContent, checkName) {
  return check(response, {
    [`${checkName}: contains expected content`]: (r) => r.body != null && r.body.includes(expectedContent)
  }, {
    content_check: checkName,
    service: 'sap-sector'
  })
}
