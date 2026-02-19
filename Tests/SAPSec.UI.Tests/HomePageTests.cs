using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class HomePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string HomePath = "/";

    #region Page Load Tests

    [Fact]
    public async Task HomePage_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(HomePath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        await NavigateToHomePage();

        var title = await Page.TitleAsync();

        title.Should().Contain("Get school improvement insights");
    }

    [Fact]
    public async Task HomePage_DisplaysMainHeading()
    {
        await NavigateToHomePage();

        var heading = await Page.Locator(Selectors.MainHeading).TextContentAsync();

        heading.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region GOV.UK Component Tests

    [Fact]
    public async Task HomePage_DisplaysGovUkHeader()
    {
        await NavigateToHomePage();

        var header = Page.Locator(Selectors.GovUkHeader);
        var isVisible = await header.IsVisibleAsync();

        isVisible.Should().BeTrue("GOV.UK header should be visible");
    }

    [Fact]
    public async Task HomePage_DisplaysDfeLogo()
    {
        await NavigateToHomePage();

        var logo = Page.Locator(Selectors.DfeLogo);
        var isVisible = await logo.IsVisibleAsync();

        isVisible.Should().BeTrue("DfE logo should be visible in the header");
    }

    [Fact]
    public async Task HomePage_HasSkipLink()
    {
        await NavigateToHomePage();

        var skipLink = Page.Locator(Selectors.SkipLink);
        var count = await skipLink.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have skip link for keyboard users");
    }

    [Fact]
    public async Task HomePage_HasFooter()
    {
        await NavigateToHomePage();

        var footer = Page.Locator(Selectors.GovUkFooter);
        var isVisible = await footer.IsVisibleAsync();

        isVisible.Should().BeTrue("GOV.UK footer should be visible");
    }

    #endregion

    #region Responsive Design Tests

    [Theory]
    [InlineData(1920, 1080, "Desktop")]
    [InlineData(768, 1024, "Tablet")]
    [InlineData(375, 667, "Mobile")]
    public async Task HomePage_IsResponsive(int width, int height, string device)
    {
        await Page.SetViewportSizeAsync(width, height);
        await NavigateToHomePage();

        var heading = Page.Locator(Selectors.MainHeading);
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue($"Heading should be visible on {device} ({width}x{height})");
    }

    [Theory]
    [InlineData(375, 667, "Mobile")]
    [InlineData(768, 1024, "Tablet")]
    public async Task HomePage_HeaderIsResponsive(int width, int height, string device)
    {
        await Page.SetViewportSizeAsync(width, height);
        await NavigateToHomePage();

        var header = Page.Locator(Selectors.GovUkHeader);
        var isVisible = await header.IsVisibleAsync();

        isVisible.Should().BeTrue($"Header should be visible on {device}");
    }

    #endregion

    #region Accessibility Tests - WCAG Compliance

    [Fact]
    public async Task HomePage_PassesWcag2ACompliance()
    {
        await NavigateToHomePage();

        var result = await AnalyzeAccessibility(WcagTags.Wcag2A);

        AssertNoViolations(result);
    }

    [Fact]
    public async Task HomePage_PassesWcag2AACompliance()
    {
        await NavigateToHomePage();

        var result = await AnalyzeAccessibility(WcagTags.Wcag2AA);

        AssertNoViolations(result);
    }

    [Fact]
    public async Task HomePage_HasNoCriticalAccessibilityViolations()
    {
        await NavigateToHomePage();

        var result = await Page.RunAxe();

        AssertNoCriticalViolations(result);
    }

    [Fact]
    public async Task HomePage_PassesBestPractices()
    {
        await NavigateToHomePage();

        var result = await AnalyzeAccessibility(WcagTags.BestPractice);

        AssertNoCriticalViolations(result);
    }

    #endregion

    #region Accessibility Tests - Document Structure

    [Fact]
    public async Task HomePage_HasValidHtmlLang()
    {
        await NavigateToHomePage();

        var html = Page.Locator("html[lang]");
        var langAttr = await html.GetAttributeAsync("lang");

        langAttr.Should().NotBeNullOrWhiteSpace("HTML should have lang attribute");
        langAttr.Should().Be("en", "Language should be English");
    }

    [Fact]
    public async Task HomePage_HasPageTitle()
    {
        await NavigateToHomePage();

        var title = await Page.TitleAsync();

        title.Should().NotBeNullOrWhiteSpace("Page should have a title");
    }

    [Fact]
    public async Task HomePage_HasMainLandmark()
    {
        await NavigateToHomePage();

        var main = Page.Locator("main, [role='main']");
        var count = await main.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have main landmark");
    }

    [Fact]
    public async Task HomePage_HasNavigationLandmark()
    {
        await NavigateToHomePage();

        var nav = Page.Locator("nav, [role='navigation']");
        var count = await nav.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have navigation landmark");
    }

    [Fact]
    public async Task HomePage_HasProperHeadingHierarchy()
    {
        await NavigateToHomePage();

        var h1Count = await Page.Locator("h1").CountAsync();

        h1Count.Should().BeGreaterThanOrEqualTo(1, "Page should have at least one h1");
    }

    #endregion

    #region Accessibility Tests - Images

    [Fact]
    public async Task HomePage_ImagesHaveAltAttributes()
    {
        await NavigateToHomePage();

        var images = Page.Locator("img");
        var count = await images.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var img = images.Nth(i);
            var alt = await img.GetAttributeAsync("alt");

            alt.Should().NotBeNull($"Image {i + 1} should have alt attribute");
        }
    }

    #endregion

    #region Accessibility Tests - Links

    [Fact]
    public async Task HomePage_LinksHaveDiscernibleText()
    {
        await NavigateToHomePage();

        var links = Page.Locator("a[href]");
        var count = await links.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var link = links.Nth(i);
            var href = await link.GetAttributeAsync("href") ?? "unknown";
            var hasDiscernibleText = await HasAccessibleName(link);

            hasDiscernibleText.Should().BeTrue(
                $"Link {i + 1} (href='{href}') should have discernible text");
        }
    }

    #endregion

    #region Accessibility Tests - Keyboard Navigation

    [Fact]
    public async Task HomePage_IsKeyboardNavigable()
    {
        await NavigateToHomePage();

        await Page.Keyboard.PressAsync("Tab");

        var focusedElement = Page.Locator(":focus");
        var count = await focusedElement.CountAsync();

        count.Should().Be(1, "Should be able to focus elements with keyboard");
    }

    [Fact]
    public async Task HomePage_SkipLinkWorksCorrectly()
    {
        await NavigateToHomePage();

        var skipLink = Page.Locator(Selectors.SkipLink);
        var count = await skipLink.CountAsync();

        if (count > 0)
        {
            var href = await skipLink.GetAttributeAsync("href");
            href.Should().StartWith("#", "Skip link should reference an anchor");
        }
    }

    [Fact]
    public async Task HomePage_FocusIsVisible()
    {
        await NavigateToHomePage();

        // Tab to first focusable element
        await Page.Keyboard.PressAsync("Tab");

        var focusedElement = Page.Locator(":focus");
        var isVisible = await focusedElement.IsVisibleAsync();

        isVisible.Should().BeTrue("Focused element should be visible");
    }

    #endregion

    #region Accessibility Tests - Color Contrast

    [Fact]
    public async Task HomePage_HasSufficientColorContrast()
    {
        await NavigateToHomePage();

        var result = await AnalyzeAccessibility(WcagTags.Wcag2AA);

        var contrastViolations = result.Violations
            .Where(v => v.Id.Contains("color-contrast"))
            .ToArray();

        contrastViolations.Should().BeEmpty("Page should have sufficient color contrast");
    }

    #endregion

    #region Accessibility Tests - Media

    [Fact]
    public async Task HomePage_HasNoAutoPlayingMedia()
    {
        await NavigateToHomePage();

        var autoPlayingMedia = Page.Locator("video[autoplay], audio[autoplay]");
        var count = await autoPlayingMedia.CountAsync();

        count.Should().Be(0, "Page should not have auto-playing media");
    }

    #endregion

    #region Component Accessibility Tests

    [Fact]
    public async Task HomePage_Header_IsAccessible()
    {
        await NavigateToHomePage();

        var result = await AnalyzeElementAccessibility(Selectors.GovUkHeader);

        AssertNoViolations(result);
    }

    [Fact]
    public async Task HomePage_Footer_IsAccessible()
    {
        await NavigateToHomePage();

        var footer = Page.Locator(Selectors.GovUkFooter);
        if (await footer.CountAsync() > 0)
        {
            var result = await AnalyzeElementAccessibility(Selectors.GovUkFooter);
            AssertNoViolations(result);
        }
    }

    [Fact]
    public async Task HomePage_Navigation_IsAccessible()
    {
        await NavigateToHomePage();

        var navElements = Page.Locator("nav");
        var count = await navElements.CountAsync();

        if (count == 0) return;

        // Analyze each nav element individually
        for (var i = 0; i < count; i++)
        {
            var nav = navElements.Nth(i);
            var result = await nav.RunAxe();

            var violations = result.Violations;
            violations.Should().BeEmpty(
                $"Navigation element {i + 1} should have no accessibility violations");
        }
    }

    #endregion

    #region Helper Methods

    private async Task NavigateToHomePage()
    {
        await Page.GotoAsync(HomePath);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForSelectorAsync("main");
    }

    private async Task<AxeResult> AnalyzeAccessibility(params string[] wcagTags)
    {
        var options = new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions
            {
                Type = "tag",
                Values = wcagTags.ToList()
            }
        };

        return await Page.RunAxe(options);
    }

    private async Task<AxeResult> AnalyzeElementAccessibility(string selector)
    {
        var locator = Page.Locator(selector).First;
        return await locator.RunAxe();
    }

    private async Task AnalyzeAllElementsAccessibility(string selector, string elementDescription)
    {
        var elements = Page.Locator(selector);
        var count = await elements.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var element = elements.Nth(i);
            var result = await element.RunAxe();

            result.Violations.Should().BeEmpty(
                $"{elementDescription} {i + 1} should have no accessibility violations");
        }
    }

    private static void AssertNoViolations(AxeResult result)
    {
        var violations = result.Violations;

        violations.Should().BeEmpty(
            $"Expected no accessibility violations but found {violations.Length}: " +
            $"{FormatViolations(violations)}");
    }

    private static void AssertNoCriticalViolations(AxeResult result)
    {
        var criticalViolations = result.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToArray();

        criticalViolations.Should().BeEmpty(
            $"Expected no critical/serious violations but found {criticalViolations.Length}: " +
            $"{FormatViolations(criticalViolations)}");
    }

    private static string FormatViolations(AxeResultItem[] violations)
    {
        if (violations.Length == 0) return "None";

        return string.Join("\n", violations.Select(v =>
            $"- [{v.Impact?.ToUpper()}] {v.Id}: {v.Description} " +
            $"(Affected: {v.Nodes.Length} elements)"));
    }

    private static async Task<bool> HasAccessibleName(ILocator element)
    {
        // Check direct text content
        var text = await element.TextContentAsync();
        if (!string.IsNullOrWhiteSpace(text)) return true;

        // Check aria-label
        var ariaLabel = await element.GetAttributeAsync("aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel)) return true;

        // Check aria-labelledby
        var ariaLabelledBy = await element.GetAttributeAsync("aria-labelledby");
        if (!string.IsNullOrWhiteSpace(ariaLabelledBy)) return true;

        // Check title attribute
        var title = await element.GetAttributeAsync("title");
        if (!string.IsNullOrWhiteSpace(title)) return true;

        // Check for nested image with alt text
        var img = element.Locator("img[alt]");
        var imgCount = await img.CountAsync();
        if (imgCount > 0)
        {
            var alt = await img.First.GetAttributeAsync("alt");
            if (!string.IsNullOrWhiteSpace(alt)) return true;
        }

        // Check for visually hidden text (screen reader only)
        var srOnly = element.Locator(".govuk-visually-hidden, .sr-only, .visually-hidden");
        var srCount = await srOnly.CountAsync();
        if (srCount > 0)
        {
            var srText = await srOnly.First.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(srText)) return true;
        }

        return false;
    }

    #endregion

    #region Constants

    private static class Selectors
    {
        public const string MainHeading = "h1";
        public const string GovUkHeader = "header.govuk-header";
        public const string GovUkFooter = "footer.govuk-footer";
        public const string DfeLogo = "header.govuk-header img[alt='Department for Education']";
        public const string SkipLink = ".govuk-skip-link, a[href='#main-content']";
    }

    private static class WcagTags
    {
        public const string Wcag2A = "wcag2a";
        public const string Wcag2AA = "wcag2aa";
        public const string Wcag2AAA = "wcag2aaa";
        public const string BestPractice = "best-practice";
    }

    #endregion
}
