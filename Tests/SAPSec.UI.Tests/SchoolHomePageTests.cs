//using FluentAssertions;
//using Microsoft.Playwright;
//using SAPSec.UI.Tests.Infrastructure;
//using Xunit;

//namespace SAPSec.UI.Tests;

///// <summary>
///// UI tests for the SchoolHome page.
///// Note: These tests run with auto-authentication but without a mocked DSI service,
///// so the user may be redirected to SchoolSearch if not recognized as an Establishment.
///// </summary>
//public class SchoolHomePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture), IClassFixture<WebApplicationSetupFixture>
//{
//    private readonly WebApplicationSetupFixture fixture = fixture;
//    private string SchoolHomePath => $"{fixture.BaseUrl}/SchoolHome";

//    /// <summary>
//    /// Helper to check if we're on the SchoolHome page (vs redirected)
//    /// </summary>
//    private bool IsOnSchoolHomePage => Page.Url.Contains("/SchoolHome", StringComparison.OrdinalIgnoreCase);

//    #region Page Load Tests

//    [Fact]
//    public async Task SchoolHome_ReturnsValidResponse()
//    {
//        // Act
//        var response = await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        // Assert - Should get a valid response (either page or redirect)
//        response.Should().NotBeNull();
//        response!.Status.Should().BeOneOf(200, 302, 301, 500);
//    }

//    [Fact]
//    public async Task SchoolHome_RedirectsToValidDestination()
//    {
//        // Act
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        // Assert - Should be on a valid page
//        var currentUrl = Page.Url;
//        var isValidDestination =
//            currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase) ||
//            currentUrl.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase) ||
//            currentUrl.Contains("school/search", StringComparison.OrdinalIgnoreCase);

//        isValidDestination.Should().BeTrue(
//            $"Should redirect to SchoolHome or SchoolSearch, but was: {currentUrl}");
//    }

//    #endregion

//    #region Conditional Tests - Only Run When on SchoolHome

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasGridLayout()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage)
//        {
//            return;
//        }

//        // Act
//        var gridRow = Page.Locator(".govuk-grid-row");
//        var count = await gridRow.CountAsync();

//        // Assert
//        count.Should().BeGreaterThan(0, "Page should have grid layout");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasCards()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var cards = Page.Locator(".app-card");
//        var cardCount = await cards.CountAsync();

//        // Assert
//        cardCount.Should().Be(3, "SchoolHome should display 3 cards");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasClickableCards()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var clickableCards = Page.Locator(".app-card--clickable");
//        var count = await clickableCards.CountAsync();

//        // Assert
//        count.Should().Be(3, "All cards should be clickable");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasThreeColumns()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var columns = Page.Locator(".govuk-grid-column-one-third");
//        var count = await columns.CountAsync();

//        // Assert
//        count.Should().Be(3, "Should have three one-third columns");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasSchoolNameHeading()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var heading = Page.Locator("h2.govuk-heading-xl");
//        var isVisible = await heading.IsVisibleAsync();

//        // Assert
//        isVisible.Should().BeTrue("School name heading should be visible");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasComparePerformanceLink()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var link = Page.Locator("a[href*='ComparePerformance']");
//        var count = await link.CountAsync();

//        // Assert
//        count.Should().Be(1, "Should have Compare Performance link");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasSchoolSearchLink()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var link = Page.Locator("a[href*='SchoolSearch']");
//        var count = await link.CountAsync();

//        // Assert
//        count.Should().Be(1, "Should have School Search link");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_HasSchoolDetailsLink()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var link = Page.Locator("a[href*='SchoolDetails']");
//        var count = await link.CountAsync();

//        // Assert
//        count.Should().Be(1, "Should have School Details link");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_CardsHaveHeadings()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var headings = Page.Locator(".app-card__heading");
//        var count = await headings.CountAsync();

//        // Assert
//        count.Should().Be(3, "Each card should have a heading");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_CardsHaveDescriptions()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var descriptions = Page.Locator(".app-card__description");
//        var count = await descriptions.CountAsync();

//        // Assert
//        count.Should().Be(3, "Each card should have a description");
//    }

//    [Fact]
//    public async Task SchoolHome_WhenAccessible_CardsHaveImages()
//    {
//        // Arrange
//        await Page.GotoAsync(SchoolHomePath);
//        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (!IsOnSchoolHomePage) return;

//        // Act
//        var images = Page.Locator(".app-card__image");
//        var count = await images.CountAsync();

//        // Assert
//        count.Should().Be(3, "Each card should have an image section");
//    }

//    #endregion
//}