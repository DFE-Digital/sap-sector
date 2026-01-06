# Security Testing

Security testing ensures the service protects data and enforces access boundaries.

## Automated security testing

Security checks in CI include:
- Snyk: Dependency and container image vulnerability scanning using Snyk (via the build step using the SNYK_TOKEN secret) to detect known vulnerabilities in open‑source packages and images.
- GitHub secret scanning: GitHub can automatically scan repos for hard‑coded credentials, tokens and other secrets and surface alerts in the Security tab. On public repositories this is free and scans the full history; on organisation repos GitHub Team/Enterprise options can enable full secret protection.
- GitHub code scanning (CodeQL): GitHub’s built‑in static analysis tool that can scan for security vulnerabilities in your code and present results as code scanning alerts in pull requests and the Security tab. It uses the CodeQL engine and can be integrated via the github/codeql-action in CI.

## Application security checks

The following are validated:
- Correct integration with DfE Sign-in
- Role-based access control (School vs LA)
- Secure cookie configuration
- Content Security Policy enforcement
- Protection against common OWASP risks

## Manual checks

Targeted manual checks are performed when authentication or data access changes:
- Direct URL access attempts
- Session expiry behaviour
- Verification that sensitive data is not logged

High or critical issues block deployment.

## Penetration testing 
We plan to schedule independent penetration tests as part of our security testing strategy to identify vulnerabilities that automated and manual tests may not catch, especially before major milestones such as pre‑production or significant changes.
