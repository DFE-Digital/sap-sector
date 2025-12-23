# Accessibility Testing

Accessibility Testing Tools & Audits
- We use Playwright with the Axe Core engine (@axe-core/playwright) to run automated accessibility checks for common issues like missing labels and contrast problems as part of our test suite. Automated checks can catch many detectable WCAG issues, but they don’t cover every success criterion or real-world interaction.
- In addition to automated and manual testing (keyboard navigation, screen reader checks, etc.), we plan periodic external accessibility audits to provide independent validation and catch issues that tools and internal tests may not surface.

## Automated testing

Automated accessibility checks are run using Playwright to identify:
- Missing labels
- Colour contrast issues
- Basic ARIA problems

## Manual testing

Manual checks include:
- Keyboard-only navigation
- Focus management
- Screen reader spot checks
- Error messages and hints

Accessibility issues are treated as high priority defects.
