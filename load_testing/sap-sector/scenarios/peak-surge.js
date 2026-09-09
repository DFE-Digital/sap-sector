export const peakSurgeScenario = {
  executor: 'ramping-vus',
  startVUs: 5,
  stages: [
    { duration: '30s', target: 20 }, // Quick ramp to moderate load
    { duration: '1m', target: 50 }, // Build to surge load (e.g. results day)
    { duration: '3m', target: 50 }, // Sustain peak
    { duration: '1m', target: 0 } // Ramp down
  ],
  gracefulRampDown: '30s',
  tags: {
    service: 'sap-sector',
    scenario: 'peak-surge',
    description: 'Surge event - 50 concurrent users'
  }
}
