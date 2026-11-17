import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'
import { randomSecondaryUrn, randomPrimaryUrn, randomAllThroughUrn } from '../data/school-urns.js'
import { authParams } from '../utils/auth.js'

export const secondarySubPages = ['ks4-headline-measures', 'attendance', 'view-similar-schools']
export const primarySubPages = ['ks2', 'attendance', 'view-similar-schools']
export const allThroughSubPages = ['ks2', 'ks4-headline-measures', 'attendance', 'view-similar-schools']

// Pass { thinkTime: false } to drop the pauses between pages, so each VU
// keeps one request in flight continuously (used by the capacity test).
function pause (options) {
  if (options.thinkTime !== false) {
    sleep(1)
  }
}

// Requires DfE Sign-in. Either run against an app instance started with
// ASPNETCORE_ENVIRONMENT=LoadTest (see load_testing/README.md), or against a
// real environment with a SESSION_COOKIE set - see utils/auth.js.
//
// The URNs in data/school-urns.js were only verified against LoadTest mode's
// JSON fixture data. Against a real environment (test/review, real Postgres),
// these URNs may not exist or may 404/differ - re-verify against real data
// before trusting results from those runs.
export function secondarySchoolJourney (environment, config, options = {}) {
  group('SAP Sector: Secondary School Journey', function () {
    const urn = randomSecondaryUrn()

    const overview = http.get(`${environment.baseUrl}/school/secondary/${urn}`, authParams())
    performanceCheck(overview, 'Secondary school overview', config.expectedResponseTimes.schoolPage)
    contentCheck(overview, 'app-overview', 'Secondary school overview')
    pause(options)

    const subPage = secondarySubPages[Math.floor(Math.random() * secondarySubPages.length)]
    const subResponse = http.get(`${environment.baseUrl}/school/secondary/${urn}/${subPage}`, authParams())
    performanceCheck(subResponse, `Secondary school - ${subPage}`, config.expectedResponseTimes.schoolPage)

    pause(options)
  })
}

export function primarySchoolJourney (environment, config, options = {}) {
  group('SAP Sector: Primary School Journey', function () {
    const urn = randomPrimaryUrn()

    const overview = http.get(`${environment.baseUrl}/school/primary/${urn}`, authParams())
    performanceCheck(overview, 'Primary school overview', config.expectedResponseTimes.schoolPage)
    contentCheck(overview, 'app-overview', 'Primary school overview')
    pause(options)

    const subPage = primarySubPages[Math.floor(Math.random() * primarySubPages.length)]
    const subResponse = http.get(`${environment.baseUrl}/school/primary/${urn}/${subPage}`, authParams())
    performanceCheck(subResponse, `Primary school - ${subPage}`, config.expectedResponseTimes.schoolPage)

    pause(options)
  })
}

export function allThroughSchoolJourney (environment, config, options = {}) {
  group('SAP Sector: All Through School Journey', function () {
    const urn = randomAllThroughUrn()

    const overview = http.get(`${environment.baseUrl}/school/all-through/${urn}`, authParams())
    performanceCheck(overview, 'All through school overview', config.expectedResponseTimes.schoolPage)
    contentCheck(overview, 'app-overview', 'All through school overview')
    pause(options)

    const subPage = allThroughSubPages[Math.floor(Math.random() * allThroughSubPages.length)]
    const subResponse = http.get(`${environment.baseUrl}/school/all-through/${urn}/${subPage}`, authParams())
    performanceCheck(subResponse, `All through school - ${subPage}`, config.expectedResponseTimes.schoolPage)

    pause(options)
  })
}