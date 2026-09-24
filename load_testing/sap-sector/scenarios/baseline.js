export const baselineScenario = {
  executor: 'ramping-vus',
  startVUs: 2,
  stages: [
    { duration: '30s', target: 10 }, // Ramp up to normal load
    { duration: '3m', target: 10 }, // Steady normal operations
    { duration: '30s', target: 0 } // Ramp down
  ],
  gracefulRampDown: '15s',
  tags: {
    service: 'sap-sector',
    scenario: 'baseline',
    description: 'Normal operations - 10 concurrent users'
  }
}
