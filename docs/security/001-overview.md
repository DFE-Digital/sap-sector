# Security Overview

This document describes the security controls implemented in the SAP Sector web application.

The service follows the cross government Secure by Design approach, which means security is
built into delivery rather than added at the end. A mapping to the ten Secure by Design
principles is included at the end of this document.

## Context

- Most of the data the service holds is published open data about schools, so the main risks
  sit around identity, session handling and the integrity of ingested data
- Authentication is delegated to DfE Sign-in, so the service stores no passwords and handles no
  credentials
- The service runs in containers on Azure Kubernetes Service, with all infrastructure defined
  in Terraform

## Authentication

Implemented in `SAPSec.Web/Authentication/DsiAuthenticationExtensions.cs`.

- OpenID Connect authorisation code flow, with tokens exchanged server side and never exposed
  to the browser
- Scopes are cleared and then set explicitly to openid, email, profile and organisation
- Token validation covers issuer, audience and lifetime, with a five minute clock skew
- The OIDC nonce cookie and correlation cookie are both set to Secure always
- HTTPS metadata is required in deployed environments
- Startup throws if `ClientId` or `ApiUri` are missing, so the service cannot start in a state
  where authentication is silently skipped
- The DfE Sign-in API client has an explicit 30 second timeout

## Authorisation

Implemented in `SAPSec.Web/Program.cs` and `SAPSec.Web/Authorization/`.

- A fallback authorisation policy requires an authenticated user for every endpoint unless it
  explicitly opts out, so access is denied by default
- A named `DsiAuthorizationPolicy` with its own requirement and handler keeps authorisation
  logic in one testable place
- Unauthenticated requests return 401 rather than redirecting
- Unauthorised requests return 403 rather than redirecting
- Role and name claim types are mapped explicitly rather than left at defaults

## Session and cookies

Implemented in `DsiAuthenticationExtensions.cs` and `Program.cs`.

- The authentication cookie `SAPSec.Auth` is HttpOnly, Secure always and SameSite Lax
- Authentication cookie lifetime comes from `TokenExpiryMinutes` with sliding expiration
- The session cookie `.SAPSec.Session` carries the same three protections
- Session idle timeout is one hour
- A global cookie policy sets Secure always for every cookie the application issues
- Antiforgery is enabled with Secure always cookies, giving cross site request forgery
  protection on form posts

## Transport security

- HTTPS redirection is enabled
- PostgreSQL connections use SSL, set through `PGSSLMODE`

## Response headers

Implemented in `SAPSec.Web/Middleware/SecurityHeadersMiddleware.cs`.

- `X-Content-Type-Options` nosniff
- `X-Frame-Options` DENY
- `X-Permitted-Cross-Domain-Policies` none
- `Referrer-Policy` strict-origin-when-cross-origin
- `X-XSS-Protection` 0, which correctly disables the legacy browser auditor
- `Strict-Transport-Security` as described above
- `Expect-CT` max-age 86400, enforce
- `Arr-Disable-Session-Affinity` true

The middleware returns early, without setting any of these headers or the Content Security
Policy, for request paths beginning `/signin-oidc`, `/signout-callback-oidc`, `/auth`,
`/home/error`, `/error` or `/health`. The comparison is a lowercased `StartsWith`, so `/health`
also covers `/healthcheck` and `/error` covers every `/error/{code}` path.

## Content Security Policy

Implemented in `SAPSec.Web/Helpers/CspHelper.cs`. `BuildPolicy` builds **two** policies and
selects between them on `environment.IsProduction()`. `ASPNETCORE_ENVIRONMENT` is set per
environment in `terraform/application/config/`, as `Production` for production, `Test` for the
test environment and `Development` for review apps. Only production takes the first branch.
Test, review apps and local development all take the second.

Common to both policies:

- The policy is nonce based. A fresh 32 byte nonce is generated per request from
  `RandomNumberGenerator`, so injected script without the current nonce will not execute
- `default-src` is self
- `object-src` is none
- `frame-ancestors` is none, giving clickjacking protection in browsers that ignore
  `X-Frame-Options`
- `base-uri` is self, preventing base tag injection
- `style-src` is self with no unsafe-inline allowance
- `font-src` is self plus data URIs
- `script-src` is self plus the request nonce, Google Tag Manager, Google Analytics, Clarity
  and `c.bing.com`
- `img-src` is self plus data URIs, Google Tag Manager, Google Analytics, Clarity,
  `c.bing.com` and `*.tile.openstreetmap.org` for map tiles

Where the two policies differ:

| Directive     | Production                                                                                     | Test, review apps and local development                                                                       |
| ------------- | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `form-action` | `'self'` plus `https://oidc.signin.education.gov.uk`                                             | `'self'` plus `https://test-oidc.signin.education.gov.uk` and `https://pp-oidc.signin.education.gov.uk`          |
| `connect-src` | `'self'` plus Google Analytics, Clarity and `c.bing.com`                                         | The same, plus `https://*.visualstudio.com/`, `ws://localhost:*`, `wss://localhost:*` and `http://localhost:*`   |

Points to note on the split:

- `form-action` names one DfE Sign-in OIDC host per policy and the two sets do not overlap.
  The production policy does not permit the test or pre-production DSI hosts, and the
  non-production policy does not permit the production DSI host
- The local development origins are in `connect-src`, not in `form-action`. Production
  permits no localhost origin and no `*.visualstudio.com` origin in any directive
- Because the branch tests `IsProduction()` only, the deployed test environment and review
  apps serve the same `connect-src` as local development, including the localhost and
  `*.visualstudio.com` origins

`CspHelper.cs` records the reason for each of the non-obvious origins in trailing comments:
`c.bing.com` is required by Clarity, `*.visualstudio.com` is used for Live Share in Visual
Studio, `ws://localhost:*` and `http://localhost:*` are used by Browsersync, and
`wss://localhost:*` is used by hot reload in Visual Studio.

## Input and output handling

- Razor views encode output by default, protecting rendered establishment names and search
  terms drawn from external datasets
- Data access uses parameterised Dapper queries, so user supplied values are never concatenated
  into SQL
- Static file serving uses an explicit content type provider rather than inferring types

## Key and secrets management

- ASP.NET Core Data Protection is configured explicitly, with keys persisted outside the
  container so cookies stay valid across pod restarts and scale events
- `DsiClientSecret`, `DsiApiSecret` and `SentryDsn` are read from Azure Key Vault at deploy time
- No secret values are committed to the repository
- Secrets reach the application as Kubernetes secrets rather than as plain configuration
- GitHub Actions authenticates to Azure using federated OIDC credentials, so there is no long
  lived service principal secret in GitHub
- See `/docs/adrs/008-secrets-management.md` for the rationale

## Container and runtime hardening

Implemented in `Dockerfile`.

- Multi stage build, so the .NET SDK, the Node toolchain and the source tree are not present in
  the final image
- The container runs as a non root user through `APP_UID`
- Frontend dependencies install with `npm ci --ignore-scripts`, stopping package install scripts
  executing during the build
- Base images come from the official Microsoft container registry

## Change control

Implemented in `.github/workflows/`.

- Every change goes through a pull request with required approval and passing checks
- Review app environments are created by adding a `deploy` label and destroyed automatically
  when the label is removed or the pull request closes
- Deployment to test is automatic on merge to main. Production requires manual approval
- Health checks gate every deployment, with up to five retries before the deploy is failed
- Terraform changes are validated by `validate-infrastructure.yml`

## Logging and monitoring

- Serilog provides structured logging, configured per environment
- Sentry captures application errors, with `SendDefaultPii` set to false so personal data is not
  sent to a third party error tracker
- Logit is enabled through the AKS application module for centralised log aggregation
- Two health endpoints exist, `/healthcheck` for Kubernetes probes and `/health` for detailed
  diagnostics
- See `/docs/operational/002-sentry-monitoring.md`

## Assurance

- An IT Health Check has been carried out. All findings were remediated.
- An OWASP Top 10 assessment is in progress
- Security testing is described in `/docs/testing/007-security-tests.md`
- Secure coding rules for the team are in `/docs/developers/security.md`

## Mapping to the Secure by Design principles

- Principle 2, source secure technology products. Container hardening, supported framework
  versions, shared DfE modules, pipeline scanning
- Principle 4, design usable security controls. Session handling, sliding expiry, 401 and 403
  behaviour
- Principle 5, build in detect and respond security. Logging and monitoring, health checks
- Principle 6, design flexible architectures. Layered solution, single point configuration of
  headers and authentication, infrastructure as code
- Principle 7, minimise the attack surface. Deny by default authorisation, container hardening,
  single exposed port
- Principle 8, defend in depth. Transport security, response headers, CSP, cookies, data
  protection, secrets management
- Principle 9, embed continuous assurance. Test suites, pipeline scanning, IT Health Check
- Principle 10, make changes securely. Change control

Principles 1 and 3 are met through governance and risk activity rather than through code, and
are recorded separately.
