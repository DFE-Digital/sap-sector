export const stressTestScenario = {
  executor: 'ramping-vus',
  startVUs: 10,
  stages: [
    { duration: '1m', target: 50 }, // Build up load
    { duration: '2m', target: 100 }, // Increase to stress level
    { duration: '3m', target: 150 }, // Maximum stress
    { duration: '2m', target: 150 }, // Sustain stress load
    { duration: '1m', target: 0 } // Ramp down
  ],
  gracefulRampDown: '30s',
  tags: {
    service: 'sap-sector',
    scenario: 'stress',
    description: 'Stress test - breaking point up to 150 concurrent users'
  }
}
