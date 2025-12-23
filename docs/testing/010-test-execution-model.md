# Test Execution Model

## Purpose

This document defines **where, when, and by whom** each type of testing is executed across the SAP Sector service lifecycle.  
It provides a single, clear view of test execution and ownership, complementing the individual test-type documents in this folder.

---

## 1. Continuous Integration (CI) – Merge into `main`

**When:** On every merge into the `main` branch  
**Who:** Automated CI pipeline (GitHub Actions)

**Tests executed:**
- Unit tests  
- Integration tests  
- Static Application Security Testing (SAST) using GitHub CodeQL  
- Dependency vulnerability scanning (Snyk)  
- GitHub secret scanning  
- Automated accessibility tests using Playwright with Axe Core  

**Purpose:**
- Catch defects early
- Enforce baseline quality, security, and accessibility standards before deployment

---

## 2. Deploy Process (Review / Test environments)

**When:** After CI passes and the application is deployed  
**Who:** CI/CD pipeline

**Tests executed:**
- Smoke tests
- Sanity checks
- End-to-end (E2E) UI tests using Playwright

**Purpose:**
- Validate that the system works correctly in a deployed environment
- Detect configuration or environment-specific issues

---

## 3. Release Process (Pre-production / Production)

**When:** Prior to production release  
**Who:** Delivery team with pipeline support

**Tests executed:**
- Acceptance testing
- Environment-specific verification
- Targeted performance testing (where applicable)

**Purpose:**
- Validate production readiness
- Reduce risk when promoting changes to production

---

## 4. Routine Testing Outside Release Cycles

**When:** Scheduled or periodic, independent of releases  
**Who:** Delivery team / platform tooling

**Tests executed:**
- Scheduled regression test runs
- Monitoring and health checks
- Periodic performance re-testing as data volumes or usage patterns change

**Purpose:**
- Detect regressions over time
- Maintain confidence as the system evolves

---

## 5. Load and Performance Testing

**Where it fits:**
- Executed in test or pre-production environments
- Prior to major releases or significant data changes
- Periodically outside release cycles

**Focus areas:**
- Key user journeys
- Response-time metrics (e.g. p50 / p95)
- Behaviour under expected and peak load

**Purpose:**
- Validate scalability and responsiveness
- Ensure acceptable performance as usage and data volumes grow

---

## 6. External Testing and Audits

**Who:** Independent external specialists

**Activities:**
- External penetration testing
- External accessibility audits and compliance reviews

**Frequency:**
- At key milestones (e.g. pre-production)
- Periodically or following significant change

**Purpose:**
- Provide independent assurance beyond internal automated testing
- Meet organisational and regulatory expectations

---

## 7. Manual Testing and Responsibilities

**Who:**
- QA / testers
- Designers and content specialists
- Business analysts and service owners
- Security teams (where applicable)

**Activities:**
- Exploratory testing
- Usability and content review
- Manual accessibility testing
- Manual security review

**Purpose:**
- Cover complex scenarios and qualitative aspects not suited to automation

---

## 8. DevOps and Infrastructure-as-Code (IaC) Testing

**When:** On infrastructure or configuration changes  
**Who:** CI/CD pipeline

**Tests executed:**
- IaC validation and linting
- Security and policy checks on infrastructure changes

**Purpose:**
- Prevent misconfiguration
- Ensure infrastructure changes meet security and operational standards

---

## 9. Unit Testing Quality: Mutation Testing and Coverage

**Mutation testing:**
- Considered as a **targeted approach** rather than a blanket requirement
- Initially applied to high-value or complex logic (e.g. calculations, mappings, reusable domain logic)
- Intended to validate that unit tests are asserting meaningful behaviour
- Proposed as a pilot activity, to be revisited when the team is fully available

**Code and branch coverage:**
- Used as a **visibility and guardrail metric**, not a primary quality target
- Thresholds or “no-regression” rules may be applied pragmatically
- Emphasis remains on **risk-based, meaningful tests**, rather than achieving numeric coverage targets

---

## Related documents

- `001-overview.md`
- `006-accessibility-tests.md`
- `007-security-tests.md`
- `008-performance-tests.md`
