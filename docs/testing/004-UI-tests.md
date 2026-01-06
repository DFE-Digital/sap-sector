# UI Testing (Playwright)

UI tests validate the end-to-end behaviour of the service from a user perspective.

## Tooling

Playwright is used for UI automation because it is:
- Reliable for modern web applications
- Fast in headless CI environments
- Well suited to accessibility checks

## Scope

UI automation focuses only on critical journeys.

### School user journey
- User signs in
- School landing page loads
- Compare school performance pages load
- Charts and data sections render correctly
- User can change school successfully

### Local Authority journey
- User signs in
- LA-specific navigation is available
- LA views render correct data
- School-only pages are not accessible

## Authentication handling

The real DfE Sign-in UI is not automated in CI.
Instead, tests use:
- Test DSI users where available, or
- A non-production authentication stub that provides the expected claims

The purpose is to verify correct behaviour once the user is authenticated.

## Execution

UI tests run:
- Against review apps for pull requests
- Against test environments on merge to main
- Nightly for full regression coverage
