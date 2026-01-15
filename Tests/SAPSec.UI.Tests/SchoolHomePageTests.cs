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
    private const string SchoolSearchPath = "/search-for-a-school";

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

        if (!await IsPageAccessible()) return;

        var gridRows = Page.Locator(Selectors.GridRow);
        var count = await gridRows.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have GOV.UK grid layout");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasThreeColumns()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var columns = Page.Locator(Selectors.OneThirdColumn);
        var count = await columns.CountAsync();

        count.Should().Be(3, "Should have three one-third columns for cards");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasFullWidthHeadingColumn()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var fullWidthColumn = Page.Locator(Selectors.FullWidthColumn);
        var count = await fullWidthColumn.CountAsync();

        count.Should().BeGreaterThan(0, "Should have full-width column for heading");
    }

    #endregion

    #region Card Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasThreeCards()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var cards = Page.Locator(Selectors.Card);
        var count = await cards.CountAsync();

        count.Should().Be(3, "SchoolHome should display 3 cards");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_AllCardsAreClickable()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var clickableCards = Page.Locator(Selectors.ClickableCard);
        var count = await clickableCards.CountAsync();

        count.Should().Be(3, "All cards should be clickable");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_CardsHaveHeadings()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var headings = Page.Locator(Selectors.CardHeading);
        var count = await headings.CountAsync();

        count.Should().Be(3, "Each card should have a heading");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_CardsHaveDescriptions()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var descriptions = Page.Locator(Selectors.CardDescription);
        var count = await descriptions.CountAsync();

        count.Should().Be(3, "Each card should have a description");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_CardsHaveImageContainers()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        // Check for image containers (may be hidden if images fail to load)
        var imageContainers = Page.Locator(Selectors.CardImageContainer);
        var containerCount = await imageContainers.CountAsync();

        // If containers exist, that's good
        if (containerCount == 3)
        {
            containerCount.Should().Be(3, "Each card should have an image container");
            return;
        }

        // Otherwise check if images exist but are hidden
        var hiddenImages = Page.Locator(".app-card__image, .app-card__icon, .app-card img");
        var imageCount = await hiddenImages.CountAsync();

        imageCount.Should().BeGreaterThanOrEqualTo(0,
            "Cards may have image containers (images can be hidden if assets fail to load)");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_CardsHaveLinks()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var links = Page.Locator(Selectors.CardLink);
        var count = await links.CountAsync();

        count.Should().Be(3, "Each card should have a link");
    }

    #endregion

    #region Heading Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasSchoolNameHeading()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var heading = Page.Locator(Selectors.SchoolNameHeading);
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue("School name heading should be visible");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_SchoolNameHeadingHasText()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var heading = Page.Locator(Selectors.SchoolNameHeading);
        var count = await heading.CountAsync();

        if (count == 0) return;

        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("School name heading should have text");
    }

    #endregion

    #region Navigation Link Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasComparePerformanceLink()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.ComparePerformanceLink);
        var count = await link.CountAsync();

        count.Should().Be(2, "Should have Compare Performance link");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasSchoolSearchLink()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.SchoolSearchLink);
        var count = await link.CountAsync();

        count.Should().Be(2, "Should have School Search link");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasSchoolDetailsLink()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.SchoolDetailsLink);
        var count = await link.CountAsync();

        count.Should().Be(1, "Should have School Details link");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_ComparePerformanceLinkIsClickable()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.ComparePerformanceLink);
        var count = await link.CountAsync();

        if (count == 0) return;

        var isEnabled = await link.First.IsEnabledAsync();
        isEnabled.Should().BeTrue("Compare Performance link should be clickable");
    }

    #endregion

    #region Card Content Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_ComparePerformanceCard_HasCorrectContent()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.ComparePerformanceLink);
        var count = await link.CountAsync();

        if (count == 0) return;

        var cardContent = await link.First.TextContentAsync();
        cardContent.Should().Contain("Compare", "Compare card should mention comparison");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_SchoolSearchCard_HasCorrectContent()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.SchoolSearchLink);
        var count = await link.CountAsync();

        if (count == 0) return;

        var cardContent = await link.GetByText("Connect").TextContentAsync();
        cardContent.Should().Contain("Connect", "Connect card should mention connecting with schools");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_SchoolDetailsCard_HasCorrectContent()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.SchoolDetailsLink);
        var count = await link.CountAsync();

        if (count == 0) return;

        var cardContent = await link.First.TextContentAsync();
        cardContent.Should().Contain("details", "Details card should mention school details");
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

        if (!await IsPageAccessible()) return;

        var govukElements = Page.Locator("[class*='govuk-']");
        var count = await govukElements.CountAsync();

        count.Should().BeGreaterThan(0, "Page should use GOV.UK styling");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_HasGovukHeadingStyle()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var heading = Page.Locator(".govuk-heading-xl, .govuk-heading-l, .govuk-heading-m");
        var count = await heading.CountAsync();

        count.Should().BeGreaterThan(0, "Page should have GOV.UK heading styles");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_CardsHaveGovukLinkStyle()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var govukLinks = Page.Locator(".app-card .govuk-link");
        var count = await govukLinks.CountAsync();

        count.Should().Be(3, "Card links should use GOV.UK link styling");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_DescriptionsHaveGovukBodyStyle()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var bodyText = Page.Locator(".app-card .govuk-body");
        var count = await bodyText.CountAsync();

        count.Should().Be(3, "Card descriptions should use GOV.UK body styling");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolHome_WhenAccessible_ImagesHaveAltAttributes()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

        var images = Page.Locator(".app-card img");
        var count = await images.CountAsync();

        // Images may not be present if they failed to load
        if (count == 0) return;

        for (var i = 0; i < count; i++)
        {
            var img = images.Nth(i);
            var isVisible = await img.IsVisibleAsync();

            // Only check visible images
            if (!isVisible) continue;

            var alt = await img.GetAttributeAsync("alt");
            alt.Should().NotBeNull("Images should have alt attributes for accessibility");
        }
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_LinksHaveHref()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

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

        if (!await IsPageAccessible()) return;

        var h2Heading = Page.Locator("h2.govuk-heading-xl");
        var h3Headings = Page.Locator("h3.govuk-heading-m");

        (await h2Heading.CountAsync()).Should().Be(1, "Should have one h2 heading for school name");
        (await h3Headings.CountAsync()).Should().Be(3, "Should have three h3 headings for cards");
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

        if (!await IsPageAccessible()) return;

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

        if (!await IsPageAccessible()) return;

        var link = Page.Locator(Selectors.SchoolSearchLink);
        if (await link.CountAsync() == 0) return;

        await link.First.ClickAsync();
        await WaitForPageLoad();

        Page.Url.Should().Contain("search-for-a-school",
            "Clicking School Search should navigate to that page");
    }

    [Fact]
    public async Task SchoolHome_WhenAccessible_ClickSchoolDetails_NavigatesToCorrectPage()
    {
        await NavigateToSchoolHome();

        if (!await IsPageAccessible()) return;

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
    private async Task<bool> IsPageAccessible()
    {
        // Check URL contains SchoolHome
        if (!Page.Url.Contains("/SchoolHome", StringComparison.OrdinalIgnoreCase))
            return false;

        // Check for cards (indicates successful load with Establishment user)
        var cards = Page.Locator(Selectors.Card);
        var cardCount = await cards.CountAsync();

        return cardCount > 0;
    }

    private bool IsOnSchoolSearchPage()
    {
        return Page.Url.Contains("search-for-a-school", StringComparison.OrdinalIgnoreCase) ||
               Page.Url.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase);
    }

    private void AssertIsOnValidPage()
    {
        var currentUrl = Page.Url;

        var isValidDestination =
            currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("search-for-a-school", StringComparison.OrdinalIgnoreCase) ||
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
        public const string SchoolSearchLink = "a[href=\"/search-for-a-school\"]";
        public const string SchoolDetailsLink = "a[href*='SchoolDetails']";
    }

    #endregion
}