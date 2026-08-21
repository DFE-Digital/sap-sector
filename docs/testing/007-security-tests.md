# Security Testing

Security testing ensures the service protects data and enforces access boundaries.

Security testing is treated as a distinct activity from functional testing. It combines
automated checks in CI, targeted manual checks when sensitive areas change, and independent
assessment carried out by specialists outside the delivery team.

## Automated security testing

The following security checks run in CI today.

- Snyk. Dependency and container image vulnerability scanning, run as part of the docker image
  build step in `build-and-deploy.yml` using the `SNYK_TOKEN` secret. It detects known
  vulnerabilities in open source packages and in the base image layers.


## Application security checks

The following are validated as part of the test suites.

- Correct integration with DfE Sign-in
- Role based access control for school and local authority journeys
- Secure cookie configuration
- Content Security Policy enforcement
- Protection against common OWASP risks

Authenticated journeys are exercised through a dedicated test authentication scheme, which is
registered only for the IntegrationTests, UITests, EndToEndTests and AccessibilityTests
environments so it can never be active in a deployed environment.

## Manual checks

Targeted manual checks are performed when authentication or data access changes.

- Direct URL access attempts against pages the user should not be able to reach
- Session expiry behaviour and what the user sees when a session ends mid journey
- Verification that sensitive data is not written to logs or sent to Sentry

High and critical security findings are treated as release blockers.

## Independent assessment

- An IT Health Check has been carried out against the service. All findings raised were
  remediated and no issues have been reported since
- An OWASP Top 10 assessment is currently in progress. The outcome will be recorded here once
  it completes
