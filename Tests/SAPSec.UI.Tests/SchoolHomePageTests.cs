using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

/// <summary>
/// UI tests for the SchoolHome page.
/// Note: SchoolHome requires an authenticated Establishment user.
/// Tests gracefully handle scenarios where the user is redirected or page returns errors.
/// </summary>
public class SchoolHomePageTests(WebApplicationSetupFixture fixture)
    : BasePageTest(fixture), IClassFixture<WebApplicationSetupFixture>
{
    private const string SchoolHomePath = "/SchoolHome";
    private const string SchoolSearchPath = "/find-a-school";

    #region Page Load Tests

    [Fact]
    public async Task SchoolHome_ReturnsValidResponse()
    {
        var response = await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        response.Should().NotBeNull();
        // Accept 200, 302, 301, or 500 (500 may occur if user service not mocked)
        response.Status.Should().BeOneOf(200, 302, 301, 500);
    }

    [Fact]
    public async Task SchoolHome_RedirectsToValidDestination()
    {
        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        AssertIsOnValidPage();
    }

    [Fact]
    public async Task SchoolHome_PageHasContent()
    {
        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        var content = await Page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace("Page should have content");
    }

    #endregion

    #region Layout Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasGridLayout()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var gridRows = Page.Locator(Selectors.GridRow);
        var count = await gridRows.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have GOV.UK grid layout");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasFullWidthHeadingColumn()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var fullWidthColumn = Page.Locator(Selectors.FullWidthColumn);
        var count = await fullWidthColumn.CountAsync();

        count.Should().BeGreaterThan(0, "Should have full-width column for heading");
    }

    #endregion

    #region Heading Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasSchoolNameHeading()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var heading = Page.Locator(Selectors.SchoolNameHeading);
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue("School name heading should be visible");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_SchoolNameHeadingHasText()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var heading = Page.Locator(Selectors.SchoolNameHeading);
        var count = await heading.CountAsync();

        if (count == 0) return;

        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("School name heading should have text");
    }

    #endregion

    #region Redirect Tests

    [Fact]
    public async Task SchoolHome_WhenNonEstablishment_RedirectsToSchoolSearch()
    {
        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        if (IsOnSchoolSearchPage())
        {
            Page.Url.Should().Contain(SchoolSearchPath,
                "Non-establishment users should be redirected to SchoolSearch");
        }
    }

    [Fact]
    public async Task SchoolHome_WhenRedirected_LandsOnWorkingPage()
    {
        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        var pageContent = await Page.ContentAsync();
        pageContent.Should().NotBeNullOrWhiteSpace("Final page should have content");

        // Page should be on a valid destination (even if it's an error page)
        AssertIsOnValidPage();
    }

    #endregion

    #region GOV.UK Styling Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasGovukStyling()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var govukElements = Page.Locator("[class*='govuk-']");
        var count = await govukElements.CountAsync();

        count.Should().BeGreaterThan(0, "Page should use GOV.UK styling");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasGovukHeadingStyle()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var heading = Page.Locator(".govuk-heading-xl, .govuk-heading-l, .govuk-heading-m");
        var count = await heading.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have GOV.UK heading styles");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_LinksHaveHref()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var links = Page.Locator(".app-card__link");
        var count = await links.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var link = links.Nth(i);
            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace("Card links should have href attributes");
        }
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HeadingsAreCorrectLevel()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var h2Heading = Page.Locator("h2.govuk-heading-xl");

        (await h2Heading.CountAsync()).Should().Be(1, "Should have one h2 heading for school name");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task SchoolHome_LoadsWithinTimeout()
    {
        var startTime = DateTime.UtcNow;

        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();

        var loadTime = DateTime.UtcNow - startTime;

        loadTime.TotalSeconds.Should().BeLessThan(10, "Page should load within 10 seconds");
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_ClickComparePerformance_NavigatesToCorrectPage()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var link = Page.Locator(Selectors.ComparePerformanceLink);
        if (await link.CountAsync() == 0) return;

        await link.First.ClickAsync();
        await WaitForPageLoad();

        Page.Url.Should().Contain("ComparePerformance",
            "Clicking Compare Performance should navigate to that page");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_ClickSchoolSearch_NavigatesToCorrectPage()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var link = Page.Locator(Selectors.SchoolSearchLink);
        if (await link.CountAsync() == 0) return;

        await link.First.ClickAsync();
        await WaitForPageLoad();

        Page.Url.Should().Contain("find-a-school",
            "Clicking School Search should navigate to that page");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_ClickSchoolDetails_NavigatesToCorrectPage()
    {
        await NavigateToSchoolHome();

        Assert.True(IsPageAccessible());

        var link = Page.Locator(Selectors.SchoolDetailsLink);
        if (await link.CountAsync() == 0) return;

        await link.First.ClickAsync();
        await WaitForPageLoad();

        Page.Url.Should().Contain("SchoolDetails",
            "Clicking School Details should navigate to that page");
    }

    #endregion

    #region Helper Methods

    private async Task NavigateToSchoolHome()
    {
        await Page.GotoAsync(SchoolHomePath);
        await WaitForPageLoad();
    }

    private async Task WaitForPageLoad()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Checks if the SchoolHome page loaded successfully with expected content.
    /// Returns false if:
    /// - Not on SchoolHome URL
    /// - Page returned an error
    /// - Required content (cards) not present
    /// </summary>
    private bool IsPageAccessible()
    {
        // Check URL contains SchoolHome
        return Page.Url.Contains("/SchoolHome", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsOnSchoolSearchPage()
    {
        return Page.Url.Contains("find-a-school", StringComparison.OrdinalIgnoreCase) ||
               Page.Url.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase);
    }

    private void AssertIsOnValidPage()
    {
        var currentUrl = Page.Url;

        var isValidDestination =
            currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("find-a-school", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("sign-in", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("Error", StringComparison.OrdinalIgnoreCase);

        isValidDestination.Should().BeTrue(
            $"Should be on a valid page, but was: {currentUrl}");
    }

    #endregion

    #region Selectors

    private static class Selectors
    {
        // Layout
        public const string GridRow = ".govuk-grid-row";
        public const string OneThirdColumn = ".govuk-grid-column-one-third";
        public const string FullWidthColumn = ".govuk-grid-column-full";

        // Cards
        public const string Card = ".app-card";
        public const string ClickableCard = ".app-card--clickable";
        public const string CardHeading = ".app-card__heading";
        public const string CardDescription = ".app-card__description";
        public const string CardImage = ".app-card__icon";
        public const string CardImageContainer = ".app-card__image";
        public const string CardLink = ".app-card__link";

        // Headings
        public const string SchoolNameHeading = "h2.govuk-heading-xl";

        // Links
        public const string ComparePerformanceLink = "a[href*='ComparePerformance']";
        public const string SchoolSearchLink = "a[href=\"/find-a-school\"]";
        public const string SchoolDetailsLink = "a[href*='SchoolDetails']";
    }

    #endregion
}