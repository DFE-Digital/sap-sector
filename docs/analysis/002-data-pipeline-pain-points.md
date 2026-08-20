# Data Pipeline Pain Points, Proposed Solutions, And Test Recommendations

## Purpose

This document summarises the main pain points identified in the current data pipeline, the proposed direction for improving them, and how the DfE data team recommendations can be used to support those improvements safely.

The focus of this review is:

- `.github/workflows/data-pipeline.yml`
- `SAPData/`
- SQL generation and raw table rebuild behavior
- operational support and pipeline stability

---

## Pain Point 1: Monolithic Workflow

### Problem

The current `data-pipeline` workflow handles too many responsibilities in one place, including:

- dataset discovery and versioning
- source file download and blob storage updates
- SQL generation
- ETL execution
- maintenance switching
- backup and restore behavior

### Impact

- failures are harder to isolate
- reruns are coarse-grained
- workflow ownership is unclear
- changes in one area can affect unrelated parts of the pipeline

### Proposed solution

Split responsibilities into smaller units over time, either as:

- smaller jobs in the workflow
- checked-in scripts called by the workflow

The first practical step is to reduce how much implementation logic is embedded directly inside the workflow YAML.

---

## Pain Point 2: Too Much Inline Workflow Logic

### Problem

A large amount of pipeline behavior is implemented as long inline PowerShell or bash blocks inside the GitHub Actions workflow.

Examples include:

- dataset versioning and source resolution
- maintenance ingress switching
- ETL retry handling

### Impact

- hard to test locally
- hard to review safely
- difficult to reuse
- debugging is slower because logic is buried in YAML

### Proposed solution

Move large logic blocks into checked-in scripts under the repo, for example:

- `SAPData/Scripts/`
- `scripts/data-pipeline/`

This makes the workflow an orchestration layer rather than the main implementation layer.

---

## Pain Point 3: Fragile Maintenance Switching

### Problem

Maintenance mode is enabled by modifying ingress definitions during the pipeline run and restoring them afterwards.

This logic depends on:

- naming conventions
- runtime service discovery
- temporary backup files
- successful cleanup behavior

### Impact

- live routing changes happen during a sensitive operation
- ingress or service naming drift can break the switch
- recovery depends on restore logic completing correctly
- support effort is higher because routing changes are hard to reason about from logs alone

### Proposed solution

Make maintenance switching more explicit and easier to validate by:

- moving the logic into dedicated scripts
- validating targets before applying changes
- improving logging and recovery behavior

---

## Pain Point 4: Runtime Data Contract Drift

### Problem

Some source payloads do not consistently match documented contracts or previous assumptions.

Examples already found during investigation:

- DSI organisation payloads differ from published API documentation
- `pimsStatus` can appear as `1`, `"1"`, or `null`
- top-level API response shape can differ from the documented wrapper shape

### Impact

- deserialization can fail unexpectedly
- some users or datasets fail while others work
- lower environments may not reproduce production behavior
- fixes become reactive instead of contract-driven

### Proposed solution

Make data parsing tolerant to observed real payloads while keeping the behavior explicit and tested.

Suggested improvements:

- introduce wrapper DTOs where appropriate
- support mixed primitive representations where required
- add focused tests for real payload variants
- treat observed runtime payloads as the operational source of truth unless the upstream provider confirms otherwise

---

## Pain Point 5: Full Rebuild Assumption For Raw Tables

### Problem

The current pipeline assumes generated raw tables should always be dropped and recreated.

This behavior exists in both:

- `00_cleanup.sql`
- `GenerateRawTables.cs`

### Impact

- unnecessary work for stable datasets
- longer pipeline runtime
- more database churn than needed
- higher operational risk because more objects are rebuilt than necessary

### Proposed solution

Introduce a controlled preserve list for selected stable raw datasets.

Spike direction already explored:

- use an explicit preserve list such as `SAPData/preserved_raw_tables.txt`
- skip dropping and recreating preserved raw tables
- skip reloading preserved raw tables during copy generation

### Important constraint

This should only be used for stable datasets.

Risks if used incorrectly:

- stale data may remain in preserved tables
- downstream views may fail if the source schema changes

This should remain an explicit allowlist, not a general bypass.

---

## Pain Point 6: Limited Observability And Weak Failure Classification

### Problem

The pipeline logs useful output, but some critical decisions still depend on console output and text matching.

Examples:

- ETL retries depend on matching known text inside `psql.log`
- maintenance troubleshooting depends on shell output
- support often requires reading long raw logs to understand the real failure point

### Impact

- slower incident diagnosis
- brittle retry classification
- harder support handover
- failure reasons are not always obvious from artifacts alone

### Proposed solution

Improve structured reporting and make stage behavior more explicit.

Suggested improvements:

- emit clearer per-stage artifacts
- record success and failure metadata
- record retry behavior explicitly
- provide concise stage summaries for:
  - what changed
  - what was skipped
  - what failed
  - what was retried

---

## DfE Data Team Recommendations And How They Support These Pain Points

The DfE data team provided the following test recommendations:

1. Dataset Discovery Tests
2. DataMap Configuration Validation Tests
3. SQL Generation Tests
4. Normalisation & Lookup Tests
5. Idempotency & Safety Tests

These recommendations align well with the pain points above.

---

## 1. Dataset Discovery Tests

### Recommendation

Examples:

- latest dataset version is selected
- malformed version strings fail clearly
- no new dataset results in a safe no-op

### Pain points supported

- Pain Point 1: Monolithic workflow
- Pain Point 2: Too much inline workflow logic
- Pain Point 4: Runtime data contract drift
- Pain Point 6: Limited observability

### Why this matters

Dataset discovery and versioning is one of the most complex and operationally important parts of the pipeline.

Good test coverage here would:

- reduce production-only surprises
- make version selection safer
- make no-op behavior predictable
- support moving versioning logic out of YAML safely

---

## 2. DataMap Configuration Validation Tests

### Recommendation

Examples:

- required columns exist in `datamap.csv`
- referenced source fields exist in mock CSV headers
- invalid datatype values are rejected
- filters without values fail validation
- `IgnoreMapping = Y` behaves as expected

### Pain points supported

- Pain Point 2: Too much inline workflow logic
- Pain Point 4: Runtime data contract drift
- Pain Point 5: Full rebuild assumption
- Pain Point 6: Limited observability

### Why this matters

The pipeline is highly configuration-driven.

If configuration problems are only discovered during SQL generation or ETL execution, failures happen too late.

These tests would provide high value by:

- catching errors earlier
- making failures clearer
- reducing wasted pipeline runs
- improving confidence in metadata-driven behavior

---

## 3. SQL Generation Tests

### Recommendation

Examples:

- correct `SELECT` clause for a mapping
- correct `WHERE` clause for filters
- correct casting based on `DataType`
- normalisation joins applied when configured

### Pain points supported

- Pain Point 1: Monolithic workflow
- Pain Point 2: Too much inline workflow logic
- Pain Point 5: Full rebuild assumption
- Pain Point 6: Limited observability

### Why this matters

SQL generation is central to the pipeline, but many issues are currently only discovered indirectly by running the full process.

These tests would:

- separate SQL generation failures from ETL failures
- make refactoring safer
- support selective rebuild approaches
- reduce the cost of changing generation logic

---

## 4. Normalisation And Lookup Tests

### Recommendation

Examples:

- known lookup values map correctly
- unknown values follow defined fallback behavior
- missing lookup definitions fail fast

### Pain points supported

- Pain Point 4: Runtime data contract drift
- Pain Point 6: Limited observability

### Why this matters

These tests protect data correctness rather than just pipeline completion.

They help ensure transformations are predictable and that subtle mapping errors do not silently corrupt outputs.

---

## 5. Idempotency And Safety Tests

### Recommendation

Examples:

- rerunning the same input produces identical output
- deletes only occur after successful completion flags
- partial failures do not corrupt prior state

### Pain points supported

- Pain Point 3: Fragile maintenance switching
- Pain Point 5: Full rebuild assumption
- Pain Point 6: Limited observability

### Why this matters

These tests are particularly important if the pipeline starts preserving selected tables or becomes more selective in rebuild behavior.

They support production stability by proving that:

- reruns are safe
- partial failures do not leave the system in a worse state
- selective preservation does not break repeatability

---

## Suggested Priority Order

If these recommendations are implemented incrementally, the suggested order is:

1. DataMap Configuration Validation Tests
2. Dataset Discovery Tests
3. SQL Generation Tests
4. Idempotency And Safety Tests
5. Normalisation And Lookup Tests

### Why this order

- the first three address the most immediate pipeline control and maintainability risks
- idempotency and safety become even more important if selective rebuild behavior is introduced
- normalisation and lookup tests remain important, but are slightly lower priority than core pipeline control tests

---

## Summary

The main pain points identified in the current data pipeline are:

1. monolithic workflow structure
2. too much inline workflow logic
3. fragile maintenance switching
4. runtime data contract drift
5. full rebuild assumption for raw tables
6. limited observability and weak failure classification

The DfE data team recommendations provide a practical testing strategy to support improvements in all of these areas.

The preserve-table idea fits especially well under:

- full rebuild reduction
- idempotency and safety
- selective, lower-risk pipeline optimization

This means the pain point analysis and the testing recommendations are complementary:

- the pain points explain where the pipeline is weak
- the recommendations explain what should be tested to make improvements safe
