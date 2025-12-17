# Security Testing

Security testing ensures the service protects data and enforces access boundaries.

## Automated security testing

Security checks in CI include:
- Dependency vulnerability scanning using Snyk
- Secret scanning
- Static analysis where available

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
