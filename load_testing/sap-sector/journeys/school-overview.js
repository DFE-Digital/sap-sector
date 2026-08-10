import http from 'k6/http'
import { group, sleep } from 'k6'
import { performanceCheck, contentCheck } from '../utils/checks.js'
import { randomSecondaryUrn, randomPrimaryUrn } from '../data/school-urns.js'

const secondarySubPages = ['ks4-headline-measures', 'attendance', 'view-similar-schools']
const primarySubPages = ['ks2', 'attendance', 'view-similar-schools']

// Requires DfE Sign-in - only meaningful when run against an app instance
// started with ASPNETCORE_ENVIRONMENT=LoadTest (see load_testing/README.md).
export function secondarySchoolJourney (environment, config) {
  group('SAP Sector: Secondary School Journey', function () {
    const urn = randomSecondaryUrn()

    const overview = http.get(`${environment.baseUrl}/school/secondary/${urn}`)
    performanceCheck(overview, 'Secondary school overview', config.expectedResponseTimes.schoolPage)
    contentCheck(overview, 'app-overview', 'Secondary school overview')
    sleep(1)

    const subPage = secondarySubPages[Math.floor(Math.random() * secondarySubPages.length)]
    const subResponse = http.get(`${environment.baseUrl}/school/secondary/${urn}/${subPage}`)
    performanceCheck(subResponse, `Secondary school - ${subPage}`, config.expectedResponseTimes.schoolPage)

    sleep(1)
  })
}

export function primarySchoolJourney (environment, config) {
  group('SAP Sector: Primary School Journey', function () {
    const urn = randomPrimaryUrn()

    const overview = http.get(`${environment.baseUrl}/school/primary/${urn}`)
    performanceCheck(overview, 'Primary school overview', config.expectedResponseTimes.schoolPage)
    contentCheck(overview, 'app-overview', 'Primary school overview')
    sleep(1)

    const subPage = primarySubPages[Math.floor(Math.random() * primarySubPages.length)]
    const subResponse = http.get(`${environment.baseUrl}/school/primary/${urn}/${subPage}`)
    performanceCheck(subResponse, `Primary school - ${subPage}`, config.expectedResponseTimes.schoolPage)

    sleep(1)
  })
}
