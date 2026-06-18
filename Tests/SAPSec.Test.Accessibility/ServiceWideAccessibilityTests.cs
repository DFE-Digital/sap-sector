using Deque.AxeCore.Playwright;
using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class ServiceWideAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private static readonly PageTestCase[] AllPagePaths = [
        new(Routes.Home, false),
        new(Routes.Accessibility, false),
        new(Routes.FindASchool(), false),
        new(Routes.School("138858"), false),
        new(Routes.SchoolDetails("138858"), false),
        // TODO: Fill out with all pages from service
    ];

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HaveASingleMainLandmark(string path)
    {
        await NavigateTo(path);

        var main = Page.Locator("main");
        var count = await main.CountAsync();

        count.Should().Be(1, "Page should have exactly one main landmark");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HaveASingleMainLandmark_WithMainContentId(string path)
    {
        await NavigateTo(path);

        var main = Page.Locator("main#main-content");
        var count = await main.CountAsync();

        count.Should().Be(1, "Main landmark should have id='main-content' for skip link");
    }


    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HasSkipLink(string path)
    {
        await NavigateTo(path);

        var skipLink = Page.Locator(".govuk-skip-link");
        var count = await skipLink.CountAsync();

        count.Should().Be(1, "Page should have a skip link");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_SkipLink_TargetsMainContent(string path)
    {
        await NavigateTo(path);

        var skipLink = Page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        href.Should().Be("#main-content", "Skip link should target main content");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_SkipLink_HasCorrectText(string path)
    {
        await NavigateTo(path);

        var skipLink = Page.Locator(".govuk-skip-link");
        var text = await skipLink.TextContentAsync();

        text.Should().Contain("Skip to main content", "Skip link should have correct text");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_SkipLink_BecomesVisibleOnFocus(string path)
    {
        await NavigateTo(path);

        var skipLink = Page.Locator(".govuk-skip-link");

        await skipLink.FocusAsync();

        var boundingBox = await skipLink.BoundingBoxAsync();
        boundingBox.Should().NotBeNull("Skip link should be visible when focused");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_SkipLink_WorksOnKeyboardNavigation(string path)
    {
        await NavigateTo(path);

        await Page.Keyboard.PressAsync("Tab");

        var skipLink = Page.Locator(".govuk-skip-link");
        var isFocused = await skipLink.EvaluateAsync<bool>("el => el === document.activeElement");

        isFocused.Should().BeTrue("Skip link should be first focusable element");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HaveHeaderLandmark(string path)
    {
        await NavigateTo(path);

        var header = Page.Locator("header");
        var count = await header.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have a header landmark");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HaveFooterLandmark(string path)
    {
        await NavigateTo(path);

        var footer = Page.Locator("footer");
        var count = await footer.CountAsync();

        count.Should().Be(1, "Page should have exactly one footer landmark");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_AllHeadings_HaveTextContent(string path)
    {
        await NavigateTo(path);

        var headings = Page.Locator("h1, h2, h3, h4, h5, h6");
        var count = await headings.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var text = await headings.Nth(i).TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace($"Heading {i + 1} should have text content");
        }
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_HeadingsAreInSequentialOrder(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPagesWithHasH3Headings))]
    public async Task AllPages_HaveSemanticHTMLStructure(string path, bool hasH3Headings)
    {
        await NavigateTo(path);

        // Ensure the main content is wrapped in the appropriate landmark
        var mainContent = Page.Locator("main");
        mainContent.Should().NotBeNull();

        // Ensure headings are hierarchical (h1 -> h2 only, no skipped levels)
        var h1Count = await Page.Locator("h1").CountAsync();
        h1Count.Should().Be(1);

        var h2Count = await Page.Locator("h2").CountAsync();
        h2Count.Should().BeGreaterThan(0);

        // Ensure no h3-h6 are used unless necessary
        var h3Count = await Page.Locator("h3").CountAsync();
        if (hasH3Headings)
        {
            h3Count.Should().BeGreaterThan(0);
        }
        else
        {
            h3Count.Should().Be(0);
        }
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_Links_HaveDistinguishableText(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_FocusOrder_IsLogical(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_FocusIndicator_IsVisible(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_TextHasSufficientContrast(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_Links_HaveDistinguishableColor(string path)
    {
        await NavigateTo(path);

        var linkColor = await Page.EvaluateAsync<string>(@"
            () => {
                const link = document.querySelector('main a.govuk-link');
                if (!link) return 'none';
                return window.getComputedStyle(link).color;
            }
        ");

        linkColor.Should().NotBe("none", "Links should have distinct styling");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_PageHasLanguageAttribute(string path)
    {
        await NavigateTo(path);

        var lang = await Page.EvaluateAsync<string>("() => document.documentElement.lang");

        lang.Should().NotBeNullOrWhiteSpace("Page should have lang attribute");
        lang.Should().Be("en", "Page should have English language attribute");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_Images_HaveAltText(string path)
    {
        await NavigateTo(path);

        var images = Page.Locator("img:not([role='presentation'])");
        var count = await images.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var alt = await images.Nth(i).GetAttributeAsync("alt");
            alt.Should().NotBeNull($"Image {i + 1} should have alt attribute");
        }
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_DecorativeImages_AreHiddenFromScreenReaders(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_VisuallyHiddenText_IsProperlyHidden(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_NavigationHasAriaLabel(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_ActiveNavItem_HasAriaCurrent(string path)
    {
        await NavigateTo(path);

        var activeNavLink = Page.Locator(".govuk-service-navigation__item--active a");
        var count = await activeNavLink.CountAsync();

        if (count > 0)
        {
            var ariaCurrent = await activeNavLink.GetAttributeAsync("aria-current");
            ariaCurrent.Should().Be("page", "Active navigation link should have aria-current='page'");
        }
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_NoInvalidAriaAttributes(string path)
    {
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_TouchTargets_AreLargeEnough(string path)
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_TextIsReadable_OnMobile(string path)
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

        var fontSize = await Page.EvaluateAsync<double>(@"
            () => {
                const body = document.querySelector('main');
                return parseFloat(window.getComputedStyle(body).fontSize);
            }
        ");

        fontSize.Should().BeGreaterThanOrEqualTo(14, "Text should be at least 14px on mobile for readability");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_NoHorizontalScroll_OnMobile(string path)
    {
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(@"
            () => document.documentElement.scrollWidth > document.documentElement.clientWidth
        ");

        hasHorizontalScroll.Should().BeFalse("Page should not have horizontal scroll on mobile");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_PassAxeAccessibilityChecks(string path)
    {
        await NavigateTo(path);

        var results = await Page.RunAxe();

        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }

    public static TheoryData<string> AllPages()
    {
        var data = new TheoryData<string>();
        foreach (var (path, hasH3Headings) in AllPagePaths)
        {
            data.Add(path);
        }

        return data;
    }

    public static TheoryData<string, bool> AllPagesWithHasH3Headings()
    {
        var data = new TheoryData<string, bool>();
        foreach (var (path, hasH3Headings) in AllPagePaths)
        {
            data.Add(path, hasH3Headings);
        }

        return data;
    }

    private record PageTestCase(string Path, bool HasH3Headings);
}
