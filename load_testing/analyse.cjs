// Reads a k6 handleSummary JSON and prints the per-step staircase.
//
//   node analyse.cjs sap-sector-load-test-summary.json [sloMs]
//
// Named .cjs so it works whether or not package.json sets "type": "module".

const fs = require('fs')

const file = process.argv[2] || 'sap-sector-load-test-summary.json'
const slo = Number(process.argv[3] || 3000)

if (!fs.existsSync(file)) {
  console.error(`Cannot find ${file}`)
  console.error('Run this from the directory k6 wrote the summary into.')
  process.exit(1)
}

const summary = JSON.parse(fs.readFileSync(file, 'utf8'))
const metrics = summary.metrics || {}
const PREFIX = 'http_req_duration{step:'

const rows = Object.entries(metrics)
  .filter(([key]) => key.startsWith(PREFIX))
  .map(([key, metric]) => {
    const step = key.slice(PREFIX.length, -1)
    const failed = metrics[`http_req_failed{step:${step}}`]
    // 'ramp' sorts first, numbered steps by VU count, 'recovery' last.
    const order = /\d/.test(step)
      ? Number(step.replace(/\D/g, ''))
      : (step === 'ramp' ? -1 : Number.MAX_SAFE_INTEGER)

    return {
      step,
      order,
      // Trend metrics in the k6 summary carry no count field, so the request
      // count comes from the paired rate metric: passes + fails. If no rate
      // sub-metric was declared for this step the count is simply unknown -
      // which is not the same as the step having no traffic.
      n: (metric.values.count != null)
        ? metric.values.count
        : (failed && failed.values
            ? (failed.values.passes || 0) + (failed.values.fails || 0)
            : null),
      med: metric.values.med || 0,
      p95: metric.values['p(95)'] || 0,
      max: metric.values.max || 0,
      failPct: ((failed && failed.values && failed.values.rate) || 0) * 100
    }
  })
  .sort((a, b) => a.order - b.order)

// A step ran if it has a request count, or - when the count is unknown -
// if its latency figures are non-zero. A declared-but-unreached step has
// zeroes throughout.
const ran = rows.filter((row) => (row.n === null ? row.p95 > 0 : row.n > 0))

if (!ran.length) {
  console.error('No populated per-step sub-metrics found.')
  console.error('Check that SCENARIO=stress and that applyStepTag() is being called.')
  process.exit(1)
}

const duration = (summary.state && summary.state.testRunDurationMs) || 0
console.log(`\n${file}  -  ${(duration / 60000).toFixed(1)} min`)
console.log(`SLO: p95 < ${slo} ms, failures < 1%\n`)

console.log('step        reqs       med ms    p95 ms    max ms    fail%   slo')
console.log('----------  ---------  --------  --------  --------  ------  ------')

for (const row of ran) {
  const ok = row.p95 < slo && row.failPct < 1
  console.log(
    row.step.padEnd(10),
    (row.n === null ? '?' : String(row.n)).padStart(9),
    row.med.toFixed(0).padStart(9),
    row.p95.toFixed(0).padStart(9),
    row.max.toFixed(0).padStart(9),
    row.failPct.toFixed(3).padStart(7),
    ok ? '   ok' : '   BREACH'
  )
}

// Steps that were declared but never got traffic. In a completed run this
// means the test stopped early - worth knowing before quoting a ceiling.
const empty = rows.filter((row) => row.n === 0 && row.p95 === 0 && /\d/.test(row.step))
if (empty.length) {
  console.log(`\nSteps with no traffic: ${empty.map((r) => r.step).join(', ')}`)
  console.log('The run did not reach these levels. Was it aborted, or Ctrl-C\'d?')
}

const numbered = ran.filter((row) => /\d/.test(row.step))
const clean = numbered.filter((row) => row.p95 < slo && row.failPct < 1)
const firstBad = numbered.find((row) => row.p95 >= slo || row.failPct >= 1)
const dropped = (metrics.dropped_iterations && metrics.dropped_iterations.values.count) || 0

console.log('\n' + '='.repeat(66))
console.log('Last level inside SLO   ', clean.length ? clean[clean.length - 1].step : 'none')
console.log('First level breaching   ', firstBad ? firstBad.step : 'none - ceiling is above the top step')
console.log('dropped_iterations      ', dropped)

if (dropped > 0) {
  console.log('\n!! Iterations were dropped. Either the service is saturated (a')
  console.log('   finding) or the load generator ran out of CPU/VUs (an artefact).')
  console.log('   Check what k6 itself was doing before quoting the top figures.')
}

if (!firstBad) {
  console.log('\nNothing breached. ramping-vus is a closed loop, so as the app slows')
  console.log('each VU offers less load - the test throttles itself exactly when it')
  console.log('should push hardest. Switch to ramping-arrival-rate to find the real')
  console.log('ceiling.')
}

const recovery = ran.find((row) => row.step === 'recovery')
if (recovery) {
  const baseline = numbered.length ? numbered[0] : null
  console.log('')
  if (baseline && recovery.p95 > baseline.p95 * 1.5) {
    console.log(`Recovery p95 ${recovery.p95.toFixed(0)} ms vs ${baseline.p95.toFixed(0)} ms at ${baseline.step}:`)
    console.log('the service did NOT return to baseline after load was removed.')
  } else {
    console.log('Recovery latency returned to baseline after load was removed.')
  }
}

console.log('')
