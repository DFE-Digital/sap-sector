const environments = {
  local: {
    baseUrl: __ENV.LOCAL_URL || 'http://localhost:3000',
    name: 'local'
  },
  review: {
    baseUrl: __ENV.REVIEW_URL || (__ENV.PR_NUMBER ? `https://sap-sector-${__ENV.PR_NUMBER}.test.education.gov.uk` : null),
    name: 'review'
  },
  test: {
    baseUrl: 'https://test.get-school-improvement-insights.education.gov.uk',
    name: 'test'
  },
  production: {
    baseUrl: 'https://get-school-improvement-insights.education.gov.uk',
    name: 'production'
  },
  loadtest: {
    // An app instance you run yourself with ASPNETCORE_ENVIRONMENT=LoadTest -
    // NEVER a shared review/test/production deployment. See load_testing/README.md.
    baseUrl: __ENV.LOADTEST_URL || 'https://localhost:5099',
    name: 'loadtest',
    insecureSkipTLSVerify: true // local ASP.NET Core dev certificate
  }
}

export function getEnvironment () {
  const env = __ENV.ENVIRONMENT || 'local'
  const environment = environments[env]

  if (!environment || !environment.baseUrl) {
    throw new Error(
      `Unknown or unconfigured environment "${env}". Set ENVIRONMENT to one of: ${Object.keys(environments).join(', ')}. ` +
      'For "review", also set REVIEW_URL or PR_NUMBER.'
    )
  }

  return environment
}

export function getConfig () {
  return {
    service: 'SAP Sector',
    expectedResponseTimes: {
      homepage: 2000,
      staticPage: 2000,
      health: 1000,
      authRedirect: 1500,
      search: 3000,
      schoolPage: 3000
    },
    thresholds: {
      http_req_duration: ['p(95)<3000'],
      http_req_failed: ['rate<0.01'],
      sap_sector_error_rate: ['rate<0.01']
    }
  }
}
