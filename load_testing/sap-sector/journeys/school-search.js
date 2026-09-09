import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'
import { authParams } from '../utils/auth.js'

// Query terms confirmed against local JSON-backed data to return real results.
const searchTerms = ['School', 'High', 'Primary', 'Academy']
const suggestTerms = ['School', 'High', 'Primary', 'Loreto']

// Requires DfE Sign-in. Either run against an app instance started with
// ASPNETCORE_ENVIRONMENT=LoadTest (see load_testing/README.md), or against a
// real environment with a SESSION_COOKIE set - see utils/auth.js.
export function schoolSearchJourney (environment, config) {
  group('SAP Sector: School Search Journey', function () {
    const landing = http.get(`${environment.baseUrl}/find-a-school`, authParams())
    performanceCheck(landing, 'Find a school - landing', config.expectedResponseTimes.search)
    contentCheck(landing, 'Search by name or school ID', 'Find a school landing')
    sleep(1)

    const term = searchTerms[Math.floor(Math.random() * searchTerms.length)]
    const results = http.get(`${environment.baseUrl}/find-a-school/search?query=${encodeURIComponent(term)}`, authParams())
    performanceCheck(results, 'Find a school - search results', config.expectedResponseTimes.search)
    contentCheck(results, 'app-school-result', 'Find a school results')
    sleep(1)

    const suggestTerm = suggestTerms[Math.floor(Math.random() * suggestTerms.length)]
    const suggest = http.get(`${environment.baseUrl}/find-a-school/suggest?queryPart=${encodeURIComponent(suggestTerm)}`, authParams())
    performanceCheck(suggest, 'Find a school - suggest', config.expectedResponseTimes.search)

    sleep(1)
  })
}
