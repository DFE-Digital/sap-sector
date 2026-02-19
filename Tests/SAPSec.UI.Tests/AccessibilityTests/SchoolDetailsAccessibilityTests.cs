using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Deque.AxeCore.Playwright;
using Xunit;

namespace SAPSec.UI.Tests.AccessibilityTests;

[Collection("UITestsCollection")]
public class SchoolDetailsAccessibilityTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string SchoolDetailsPath = "/school/147788/school-details";

    private async Task NavigateToSchoolDetailsAsync()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForSelectorAsync("main");
    }

    #region Axe Core Accessibility Tests

    [Fact]
    public async Task SchoolDetails_PassesAxeAccessibilityChecks()
    {
        await NavigateToSchoolDetailsAsync();

        var results = await Page.RunAxe();

        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }
    #endregion

    #region Landmark Tests

    [Fact]
    public async Task SchoolDetails_HasMainLandmark()
    {
        await NavigateToSchoolDetailsAsync();

        var main = Page.Locator("main");
        var count = await main.CountAsync();

        count.Should().Be(1, "Page should have exactly one main landmark");
    }

    [Fact]
    public async Task SchoolDetails_HasHeaderLandmark()
    {
        await NavigateToSchoolDetailsAsync();

        var header = Page.Locator("header");
        var count = await header.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have a header landmark");
    }

    [Fact]
    public async Task SchoolDetails_HasFooterLandmark()
    {
        await NavigateToSchoolDetailsAsync();

        var footer = Page.Locator("footer");
        var count = await footer.CountAsync();

        count.Should().Be(1, "Page should have exactly one footer landmark");
    }

    [Fact]
    public async Task SchoolDetails_MainLandmark_HasId()
    {
        await NavigateToSchoolDetailsAsync();

        var main = Page.Locator("main#main-content");
        var count = await main.CountAsync();

        count.Should().Be(1, "Main landmark should have id='main-content' for skip link");
    }

    #endregion

    #region Heading Structure Tests

    [Fact]
    public async Task SchoolDetails_HasExactlyOneH1()
    {
        await NavigateToSchoolDetailsAsync();

        var h1Elements = Page.Locator("h1");
        var count = await h1Elements.CountAsync();

        count.Should().Be(1, "Page should have exactly one h1 heading");
    }

    [Fact]
    public async Task SchoolDetails_H1_IsNotEmpty()
    {
        await NavigateToSchoolDetailsAsync();

        var h1 = Page.Locator("h1");
        var text = await h1.TextContentAsync();

        text.Should().NotBeNullOrWhiteSpace("H1 heading should have text content");
    }

    [Fact]
    public async Task SchoolDetails_HeadingsAreInSequentialOrder()
    {
        await NavigateToSchoolDetailsAsync();

        // Get all heading levels
        var headings = await Page.EvaluateAsync<int[]>(@"
            () => {
                const headings = document.querySelectorAll('h1, h2, h3, h4, h5, h6');
                return Array.from(headings).map(h => parseInt(h.tagName.substring(1)));
            }
        ");

        // Check that headings don't skip levels
        for (var i = 1; i < headings!.Length; i++)
        {
            var diff = headings[i] - headings[i - 1];
            diff.Should().BeLessThanOrEqualTo(1,
                $"Heading levels should not skip (found h{headings[i - 1]} followed by h{headings[i]})");
        }
    }

    [Fact]
    public async Task SchoolDetails_SectionHeadings_AreH2()
    {
        await NavigateToSchoolDetailsAsync();

        var sectionHeadings = Page.Locator("h2.govuk-heading-m");
        var count = await sectionHeadings.CountAsync();

        count.Should().BeGreaterThanOrEqualTo(4,
            "Should have h2 headings for Location, School details, Contact details, and Further information");
    }

    [Fact]
    public async Task SchoolDetails_AllHeadings_HaveTextContent()
    {
        await NavigateToSchoolDetailsAsync();

        var headings = Page.Locator("h1, h2, h3, h4, h5, h6");
        var count = await headings.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var text = await headings.Nth(i).TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace($"Heading {i + 1} should have text content");
        }
    }

    #endregion

    #region Skip Link Tests

    [Fact]
    public async Task SchoolDetails_HasSkipLink()
    {
        await NavigateToSchoolDetailsAsync();

        var skipLink = Page.Locator(".govuk-skip-link");
        var count = await skipLink.CountAsync();

        count.Should().Be(1, "Page should have a skip link");
    }

    [Fact]
    public async Task SchoolDetails_SkipLink_TargetsMainContent()
    {
        await NavigateToSchoolDetailsAsync();

        var skipLink = Page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        href.Should().Be("#main-content", "Skip link should target main content");
    }

    [Fact]
    public async Task SchoolDetails_SkipLink_HasCorrectText()
    {
        await NavigateToSchoolDetailsAsync();

        var skipLink = Page.Locator(".govuk-skip-link");
        var text = await skipLink.TextContentAsync();

        text.Should().Contain("Skip to main content", "Skip link should have correct text");
    }

    [Fact]
    public async Task SchoolDetails_SkipLink_BecomesVisibleOnFocus()
    {
        await NavigateToSchoolDetailsAsync();

        var skipLink = Page.Locator(".govuk-skip-link");

        await skipLink.FocusAsync();

        var boundingBox = await skipLink.BoundingBoxAsync();
        boundingBox.Should().NotBeNull("Skip link should be visible when focused");
    }

    [Fact]
    public async Task SchoolDetails_SkipLink_WorksOnKeyboardNavigation()
    {
        await NavigateToSchoolDetailsAsync();

        await Page.Keyboard.PressAsync("Tab");

        var skipLink = Page.Locator(".govuk-skip-link");
        var isFocused = await skipLink.EvaluateAsync<bool>("el => el === document.activeElement");

        isFocused.Should().BeTrue("Skip link should be first focusable element");
    }

    #endregion

    #region Link Accessibility Tests

    [Fact]
    public async Task SchoolDetails_Links_HaveDistinguishableText()
    {
        await NavigateToSchoolDetailsAsync();

        var links = Page.Locator("main a");
        var count = await links.CountAsync();
        var linkTexts = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var text = await links.Nth(i).TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                linkTexts.Add(text.Trim());
            }
        }

        linkTexts.Should().NotContain("click here", "Links should not use 'click here' text");
        linkTexts.Should().NotContain("here", "Links should not use 'here' text alone");
        linkTexts.Should().NotContain("read more", "Links should not use 'read more' text alone");
    }

    #endregion

    #region Summary List Accessibility Tests

    [Fact]
    public async Task SchoolDetails_SummaryLists_UseDefinitionListMarkup()
    {
        await NavigateToSchoolDetailsAsync();

        var summaryLists = Page.Locator(".govuk-summary-list");
        var count = await summaryLists.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var tagName = await summaryLists.Nth(i).EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            tagName.Should().Be("dl", $"Summary list {i + 1} should use <dl> element");
        }
    }

    [Fact]
    public async Task SchoolDetails_SummaryListKeys_UseDtElement()
    {
        await NavigateToSchoolDetailsAsync();

        var keys = Page.Locator(".govuk-summary-list__key");
        var count = await keys.CountAsync();

        for (var i = 0; i < Math.Min(count, 10); i++)
        {
            var tagName = await keys.Nth(i).EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            tagName.Should().Be("dt", $"Summary list key {i + 1} should use <dt> element");
        }
    }

    [Fact]
    public async Task SchoolDetails_SummaryListValues_UseDdElement()
    {
        await NavigateToSchoolDetailsAsync();

        var values = Page.Locator(".govuk-summary-list__value");
        var count = await values.CountAsync();

        for (var i = 0; i < Math.Min(count, 10); i++)
        {
            var tagName = await values.Nth(i).EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            tagName.Should().Be("dd", $"Summary list value {i + 1} should use <dd> element");
        }
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public async Task SchoolDetails_FocusOrder_IsLogical()
    {
        await NavigateToSchoolDetailsAsync();

        var focusableElements = await Page.EvaluateAsync<string[]>(@"
            () => {
                const elements = document.querySelectorAll('a, button, input, select, textarea, [tabindex]:not([tabindex=""-1""])');
                return Array.from(elements)
                    .filter(el => el.offsetParent !== null) // visible elements only
                    .map(el => el.tagName + (el.className ? '.' + el.className.split(' ')[0] : ''));
            }
        ");

        focusableElements.Should().NotBeEmpty("Page should have focusable elements");

        focusableElements![0].Should().Contain("skip", "Skip link should be first focusable element");
    }

    [Fact]
    public async Task SchoolDetails_FocusIndicator_IsVisible()
    {
        await NavigateToSchoolDetailsAsync();

        var firstLink = Page.Locator("main a").First;
        await firstLink.FocusAsync();

        var hasVisibleFocus = await firstLink.EvaluateAsync<bool>(@"
            el => {
                const styles = window.getComputedStyle(el);
                const outline = styles.outline;
                const boxShadow = styles.boxShadow;
                return (outline && outline !== 'none' && outline !== '0px none rgb(0, 0, 0)') ||
                       (boxShadow && boxShadow !== 'none');
            }
        ");

        hasVisibleFocus.Should().BeTrue("Focused elements should have visible focus indicator");
    }

    #endregion

    #region Color Contrast Tests

    [Fact]
    public async Task SchoolDetails_TextHasSufficientContrast()
    {
        await NavigateToSchoolDetailsAsync();

        var contrastRatio = await Page.EvaluateAsync<double>(@"
            () => {
                function getLuminance(r, g, b) {
                    const [rs, gs, bs] = [r, g, b].map(c => {
                        c = c / 255;
                        return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
                    });
                    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
                }

                function getContrastRatio(l1, l2) {
                    const lighter = Math.max(l1, l2);
                    const darker = Math.min(l1, l2);
                    return (lighter + 0.05) / (darker + 0.05);
                }

                function parseColor(color) {
                    const match = color.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/);
                    if (match) {
                        return [parseInt(match[1]), parseInt(match[2]), parseInt(match[3])];
                    }
                    return [0, 0, 0];
                }

                const body = document.body;
                const bodyStyles = window.getComputedStyle(body);
                const textColor = parseColor(bodyStyles.color);
                const bgColor = parseColor(bodyStyles.backgroundColor || 'rgb(255,255,255)');

                const textLuminance = getLuminance(...textColor);
                const bgLuminance = getLuminance(...bgColor);

                return getContrastRatio(textLuminance, bgLuminance);
            }
        ");

        contrastRatio.Should().BeGreaterThanOrEqualTo(4.5,
            "Text should have at least 4.5:1 contrast ratio (WCAG AA)");
    }

    [Fact]
    public async Task SchoolDetails_Links_HaveDistinguishableColor()
    {
        await NavigateToSchoolDetailsAsync();

        var linkColor = await Page.EvaluateAsync<string>(@"
            () => {
                const link = document.querySelector('main a.govuk-link');
                if (!link) return 'none';
                return window.getComputedStyle(link).color;
            }
        ");

        linkColor.Should().NotBe("none", "Links should have distinct styling");
    }

    #endregion

    #region Screen Reader Tests

    [Fact]
    public async Task SchoolDetails_PageHasLanguageAttribute()
    {
        await NavigateToSchoolDetailsAsync();

        var lang = await Page.EvaluateAsync<string>("() => document.documentElement.lang");

        lang.Should().NotBeNullOrWhiteSpace("Page should have lang attribute");
        lang.Should().Be("en", "Page should have English language attribute");
    }

    [Fact]
    public async Task SchoolDetails_Images_HaveAltText()
    {
        await NavigateToSchoolDetailsAsync();

        var images = Page.Locator("img:not([role='presentation'])");
        var count = await images.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var alt = await images.Nth(i).GetAttributeAsync("alt");
            alt.Should().NotBeNull($"Image {i + 1} should have alt attribute");
        }
    }

    [Fact]
    public async Task SchoolDetails_DecorativeImages_AreHiddenFromScreenReaders()
    {
        await NavigateToSchoolDetailsAsync();

        var decorativeImages = Page.Locator("img[role='presentation'], img[alt='']");
        var count = await decorativeImages.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ariaHidden = await decorativeImages.Nth(i).GetAttributeAsync("aria-hidden");
            var alt = await decorativeImages.Nth(i).GetAttributeAsync("alt");

            var isHidden = ariaHidden == "true" || alt == "";
            isHidden.Should().BeTrue($"Decorative image {i + 1} should be hidden from screen readers");
        }
    }

    [Fact]
    public async Task SchoolDetails_VisuallyHiddenText_IsProperlyHidden()
    {
        await NavigateToSchoolDetailsAsync();

        var visuallyHidden = Page.Locator(".govuk-visually-hidden");
        var count = await visuallyHidden.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have visually hidden text for screen readers");

        if (count > 0)
        {
            var isInDom = await visuallyHidden.First.EvaluateAsync<bool>("el => !!el.offsetParent || el.offsetWidth > 0");
            var boundingBox = await visuallyHidden.First.BoundingBoxAsync();

            (boundingBox == null || boundingBox.Width <= 1 || boundingBox.Height <= 1).Should().BeTrue(
                "Visually hidden text should not take up visible space");
        }
    }

    #endregion

    #region ARIA Tests

    [Fact]
    public async Task SchoolDetails_NavigationHasAriaLabel()
    {
        await NavigateToSchoolDetailsAsync();

        var navs = Page.Locator("nav");
        var count = await navs.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ariaLabel = await navs.Nth(i).GetAttributeAsync("aria-label");
            var ariaLabelledBy = await navs.Nth(i).GetAttributeAsync("aria-labelledby");

            var hasLabel = !string.IsNullOrWhiteSpace(ariaLabel) || !string.IsNullOrWhiteSpace(ariaLabelledBy);
            hasLabel.Should().BeTrue($"Navigation {i + 1} should have aria-label or aria-labelledby");
        }
    }

    [Fact]
    public async Task SchoolDetails_ActiveNavItem_HasAriaCurrent()
    {
        await NavigateToSchoolDetailsAsync();

        var activeNavLink = Page.Locator(".govuk-service-navigation__item--active a");
        var count = await activeNavLink.CountAsync();

        if (count > 0)
        {
            var ariaCurrent = await activeNavLink.GetAttributeAsync("aria-current");
            ariaCurrent.Should().Be("page", "Active navigation link should have aria-current='page'");
        }
    }

    [Fact]
    public async Task SchoolDetails_NoInvalidAriaAttributes()
    {
        await NavigateToSchoolDetailsAsync();

        var invalidAria = await Page.EvaluateAsync<string[]>(@"
            () => {
                const validAriaAttributes = [
                    'aria-activedescendant', 'aria-atomic', 'aria-autocomplete', 'aria-busy',
                    'aria-checked', 'aria-colcount', 'aria-colindex', 'aria-colspan', 'aria-controls',
                    'aria-current', 'aria-describedby', 'aria-description', 'aria-details',
                    'aria-disabled', 'aria-dropeffect', 'aria-errormessage', 'aria-expanded',
                    'aria-flowto', 'aria-grabbed', 'aria-haspopup', 'aria-hidden', 'aria-invalid',
                    'aria-keyshortcuts', 'aria-label', 'aria-labelledby', 'aria-level', 'aria-live',
                    'aria-modal', 'aria-multiline', 'aria-multiselectable', 'aria-orientation',
                    'aria-owns', 'aria-placeholder', 'aria-posinset', 'aria-pressed', 'aria-readonly',
                    'aria-relevant', 'aria-required', 'aria-roledescription', 'aria-rowcount',
                    'aria-rowindex', 'aria-rowspan', 'aria-selected', 'aria-setsize', 'aria-sort',
                    'aria-valuemax', 'aria-valuemin', 'aria-valuenow', 'aria-valuetext'
                ];

                const invalid = [];
                document.querySelectorAll('*').forEach(el => {
                    Array.from(el.attributes).forEach(attr => {
                        if (attr.name.startsWith('aria-') && !validAriaAttributes.includes(attr.name)) {
                            invalid.push(`${el.tagName}: ${attr.name}`);
                        }
                    });
                });
                return invalid;
            }
        ");

        invalidAria.Should().BeEmpty("Page should not have invalid ARIA attributes");
    }

    #endregion

    #region Mobile Accessibility Tests

    [Fact]
    public async Task SchoolDetails_TouchTargets_AreLargeEnough()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateToSchoolDetailsAsync();

        var links = Page.Locator("main a");
        var count = await links.CountAsync();

        for (var i = 0; i < Math.Min(count, 5); i++)
        {
            var boundingBox = await links.Nth(i).BoundingBoxAsync();

            if (boundingBox != null)
            {
                (boundingBox.Height >= 24 || boundingBox.Width >= 24).Should().BeTrue(
                    $"Link {i + 1} should have adequate touch target size");
            }
        }
    }

    [Fact]
    public async Task SchoolDetails_TextIsReadable_OnMobile()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateToSchoolDetailsAsync();

        var fontSize = await Page.EvaluateAsync<double>(@"
            () => {
                const body = document.querySelector('main');
                return parseFloat(window.getComputedStyle(body).fontSize);
            }
        ");

        fontSize.Should().BeGreaterThanOrEqualTo(14, "Text should be at least 14px on mobile for readability");
    }

    [Fact]
    public async Task SchoolDetails_NoHorizontalScroll_OnMobile()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateToSchoolDetailsAsync();

        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(@"
            () => document.documentElement.scrollWidth > document.documentElement.clientWidth
        ");

        hasHorizontalScroll.Should().BeFalse("Page should not have horizontal scroll on mobile");
    }

    #endregion
}
public class AxeResults
{
    public List<AxeViolation> Violations { get; set; } = new();
}

public class AxeViolation
{
    public string Id { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Help { get; set; } = string.Empty;
}

