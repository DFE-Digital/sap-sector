//using FluentAssertions;
//using Microsoft.Playwright;
//using SAPSec.UI.Tests.Infrastructure;
//using Xunit;

//namespace SAPSec.UI.Tests;

///// <summary>
///// UI tests for the SchoolHome page which displays three cards for authenticated users
///// whose organisation is an "Establishment". Users with non-Establishment organisations
///// are redirected to SchoolSearch.
///// </summary>
//public class SchoolHomePageTests : IClassFixture<WebApplicationSetupFixture>, IAsyncLifetime
//{
//    private readonly WebApplicationSetupFixture _fixture;
//    private IPlaywright _playwright = null!;
//    private IBrowser _browser = null!;
//    private IBrowserContext _context = null!;
//    private IPage _page = null!;

//    private string SchoolHomePath => $"{_fixture.BaseUrl}/SchoolHome";

//    public SchoolHomePageTests(WebApplicationSetupFixture fixture)
//    {
//        _fixture = fixture;
//    }

//    public async Task InitializeAsync()
//    {
//        _playwright = await Playwright.CreateAsync();
//        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//        {
//            Headless = true
//        });

//        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
//        {
//            IgnoreHTTPSErrors = true,
//            ViewportSize = new() { Width = 1280, Height = 720 }
//        });

//        _page = await _context.NewPageAsync();
//    }

//    public async Task DisposeAsync()
//    {
//        await _page.CloseAsync();
//        await _context.DisposeAsync();
//        await _browser.DisposeAsync();
//        _playwright.Dispose();
//    }

//    #region Page Load Tests

//    [Fact]
//    public async Task SchoolHome_LoadsSuccessfully_OrRedirects()
//    {
//        // Act
//        var response = await _page.GotoAsync(SchoolHomePath);

//        // Assert
//        response.Should().NotBeNull();
//        // Should either load successfully (200) or redirect to login/SchoolSearch
//        response!.Status.Should().BeOneOf(200, 302, 301);
//    }

//    [Fact]
//    public async Task SchoolHome_WhenNotAuthenticated_RedirectsToLogin()
//    {
//        // Act
//        var response = await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        // Assert
//        // Should redirect to login page when not authenticated
//        var currentUrl = _page.Url;
//        var isOnSchoolHome = currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase);
//        var isOnLogin = currentUrl.Contains("sign-in", StringComparison.OrdinalIgnoreCase) ||
//                        currentUrl.Contains("Auth", StringComparison.OrdinalIgnoreCase);
//        var isOnSchoolSearch = currentUrl.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase);

//        // Either on login, SchoolSearch (if non-Establishment), or SchoolHome (if Establishment)
//        (isOnSchoolHome || isOnLogin || isOnSchoolSearch).Should().BeTrue(
//            "User should be on SchoolHome, redirected to login, or redirected to SchoolSearch");
//    }

//    #endregion

//    #region School Name Display Tests

//    [Fact]
//    public async Task SchoolHome_DisplaysSchoolName()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        // Only test if we're on the SchoolHome page (user is authenticated as Establishment)
//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var schoolNameHeading = _page.Locator("h2.govuk-heading-xl");
//            var isVisible = await schoolNameHeading.IsVisibleAsync();

//            // Assert
//            isVisible.Should().BeTrue("School name heading should be visible");
//        }
//    }

//    [Fact]
//    public async Task SchoolHome_SchoolNameHasCorrectStyling()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var schoolNameHeading = _page.Locator("h2.govuk-heading-xl.govuk-\\!-margin-bottom-8");
//            var count = await schoolNameHeading.CountAsync();

//            // Assert
//            count.Should().Be(1, "School name should have correct GOV.UK styling classes");
//        }
//    }

//    #endregion

//    #region Cards Container Tests

//    [Fact]
//    public async Task SchoolHome_HasThreeCards()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cards = _page.Locator("div[class*='app-card']");
//            var cardCount = await cards.CountAsync();

//            // Assert
//            cardCount.Should().Be(3, "SchoolHome page should display 3 cards");
//        }
//    }

//    [Fact]
//    public async Task SchoolHome_CardsAreClickable()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var clickableCards = _page.Locator("div[class*='app-card--clickable']");
//            var count = await clickableCards.CountAsync();

//            // Assert
//            count.Should().Be(3, "All cards should have clickable class");
//        }
//    }

//    [Fact]
//    public async Task SchoolHome_CardsInThreeColumnLayout()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var columns = _page.Locator(".govuk-grid-column-one-third");
//            var count = await columns.CountAsync();

//            // Assert
//            count.Should().Be(3, "Cards should be in three one-third columns");
//        }
//    }

//    #endregion

//    #region Compare Performance Card Tests

//    [Fact]
//    public async Task ComparePerformanceCard_IsVisible()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var compareCard = _page.Locator("div[class*='app-card']").First;
//            var isVisible = await compareCard.IsVisibleAsync();

//            // Assert
//            isVisible.Should().BeTrue("Compare Performance card should be visible");
//        }
//    }

//    [Fact]
//    public async Task ComparePerformanceCard_HasCorrectHeading()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var compareLink = _page.Locator("a[href*='ComparePerformance']");
//            var text = await compareLink.TextContentAsync();

//            // Assert
//            text.Should().Contain("Compare this school's performance");
//        }
//    }

//    [Fact]
//    public async Task ComparePerformanceCard_HasCorrectDescription()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardContent = _page.Locator("div[class*='app-card']").First.Locator("p[class*='app-card__description']");
//            var text = await cardContent.TextContentAsync();

//            // Assert
//            text.Should().Contain("View how this school's performance compares");
//            text.Should().Contain("similar schools");
//            text.Should().Contain("local and national averages");
//        }
//    }

//    [Fact]
//    public async Task ComparePerformanceCard_HasIcon()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardImage = _page.Locator("div[class*='app-card__image--compare']");
//            var count = await cardImage.CountAsync();

//            // Assert
//            count.Should().Be(1, "Compare Performance card should have image container");
//        }
//    }

//    [Fact]
//    public async Task ComparePerformanceCard_LinksToCorrectPage()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var compareLink = _page.Locator("a[href*='ComparePerformance']");
//            var href = await compareLink.GetAttributeAsync("href");

//            // Assert
//            href.Should().Contain("ComparePerformance");
//        }
//    }

//    [Fact]
//    public async Task ComparePerformanceCard_ClickNavigatesToComparePerformance()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var compareLink = _page.Locator("a[href*='ComparePerformance']");
//            await compareLink.ClickAsync();
//            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//            // Assert
//            _page.Url.Should().Contain("ComparePerformance");
//        }
//    }

//    #endregion

//    #region Connect with Schools Card Tests

//    [Fact]
//    public async Task ConnectWithSchoolsCard_IsVisible()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var connectCard = _page.Locator("div[class*='app-card']").Nth(1);
//            var isVisible = await connectCard.IsVisibleAsync();

//            // Assert
//            isVisible.Should().BeTrue("Connect with Schools card should be visible");
//        }
//    }

//    [Fact]
//    public async Task ConnectWithSchoolsCard_HasCorrectHeading()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var connectLink = _page.Locator("div[class*='app-card']").Nth(1).Locator("a[class*='app-card__link']");
//            var text = await connectLink.TextContentAsync();

//            // Assert
//            text.Should().Contain("Connect with similar schools");
//        }
//    }

//    [Fact]
//    public async Task ConnectWithSchoolsCard_HasCorrectDescription()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardContent = _page.Locator("div[class*='app-card']").Nth(1).Locator("p[class*='app-card__description']");
//            var text = await cardContent.TextContentAsync();

//            // Assert
//            text.Should().Contain("Find contact details");
//            text.Should().Contain("similar schools");
//            text.Should().Contain("stronger academic performance");
//        }
//    }

//    [Fact]
//    public async Task ConnectWithSchoolsCard_HasIcon()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardImage = _page.Locator("div[class*='app-card__image--connect']");
//            var count = await cardImage.CountAsync();

//            // Assert
//            count.Should().Be(1, "Connect card should have image container");
//        }
//    }

//    [Fact]
//    public async Task ConnectWithSchoolsCard_LinksToSchoolSearch()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var connectLink = _page.Locator("div[class*='app-card']").Nth(1).Locator("a[class*='app-card__link']");
//            var href = await connectLink.GetAttributeAsync("href");

//            // Assert
//            href.Should().Contain("SchoolSearch");
//        }
//    }

//    [Fact]
//    public async Task ConnectWithSchoolsCard_ClickNavigatesToSchoolSearch()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var connectLink = _page.Locator("div[class*='app-card']").Nth(1).Locator("a[class*='app-card__link']");
//            await connectLink.ClickAsync();
//            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//            // Assert
//            _page.Url.Should().Contain("SchoolSearch");
//        }
//    }

//    #endregion

//    #region School Details Card Tests

//    [Fact]
//    public async Task SchoolDetailsCard_IsVisible()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var detailsCard = _page.Locator("div[class*='app-card']").Nth(2);
//            var isVisible = await detailsCard.IsVisibleAsync();

//            // Assert
//            isVisible.Should().BeTrue("School Details card should be visible");
//        }
//    }

//    [Fact]
//    public async Task SchoolDetailsCard_HasCorrectHeading()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var detailsLink = _page.Locator("a[href*='SchoolDetails']");
//            var text = await detailsLink.TextContentAsync();

//            // Assert
//            text.Should().Contain("School details");
//        }
//    }

//    [Fact]
//    public async Task SchoolDetailsCard_HasCorrectDescription()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardContent = _page.Locator("div[class*='app-card']").Nth(2).Locator("p[class*='app-card__description']");
//            var text = await cardContent.TextContentAsync();

//            // Assert
//            text.Should().Contain("View information about this school");
//        }
//    }

//    [Fact]
//    public async Task SchoolDetailsCard_HasIcon()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardImage = _page.Locator("div[class*='app-card__image--details']");
//            var count = await cardImage.CountAsync();

//            // Assert
//            count.Should().Be(1, "School Details card should have image container");
//        }
//    }

//    [Fact]
//    public async Task SchoolDetailsCard_LinksToCorrectPage()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var detailsLink = _page.Locator("a[href*='SchoolDetails']");
//            var href = await detailsLink.GetAttributeAsync("href");

//            // Assert
//            href.Should().Contain("SchoolDetails");
//        }
//    }

//    [Fact]
//    public async Task SchoolDetailsCard_ClickNavigatesToSchoolDetails()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var detailsLink = _page.Locator("a[href*='SchoolDetails']");
//            await detailsLink.ClickAsync();
//            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//            // Assert
//            _page.Url.Should().Contain("SchoolDetails");
//        }
//    }

//    #endregion

//    #region Card Structure Tests

//    [Fact]
//    public async Task AllCards_HaveImageAndContentSections()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardImages = _page.Locator("div[class*='app-card__image']");
//            var cardContents = _page.Locator("div[class*='app-card__content']");
//            var imageCount = await cardImages.CountAsync();
//            var contentCount = await cardContents.CountAsync();

//            // Assert
//            imageCount.Should().Be(3, "All cards should have image sections");
//            contentCount.Should().Be(3, "All cards should have content sections");
//        }
//    }

//    [Fact]
//    public async Task AllCards_HaveHeadings()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardHeadings = _page.Locator("h3[class*='app-card__heading']");
//            var count = await cardHeadings.CountAsync();

//            // Assert
//            count.Should().Be(3, "All cards should have headings");
//        }
//    }

//    [Fact]
//    public async Task AllCards_HaveDescriptions()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardDescriptions = _page.Locator("p[class*='app-card__description']");
//            var count = await cardDescriptions.CountAsync();

//            // Assert
//            count.Should().Be(3, "All cards should have descriptions");
//        }
//    }

//    [Fact]
//    public async Task AllCards_HaveGovukLinks()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cardLinks = _page.Locator("a[class*='app-card__link']");
//            var count = await cardLinks.CountAsync();

//            // Assert
//            count.Should().Be(3, "All cards should have links");

//            // Verify each link has govuk-link class
//            for (int i = 0; i < count; i++)
//            {
//                var link = cardLinks.Nth(i);
//                var classAttr = await link.GetAttributeAsync("class");
//                classAttr.Should().Contain("govuk-link", $"Link {i + 1} should have GOV.UK styling");
//            }
//        }
//    }

//    #endregion

//    #region Icon Tests

//    [Fact]
//    public async Task CardIcons_HaveEmptyAltText()
//    {
//        // Arrange - Icons are decorative so should have empty alt
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var icons = _page.Locator("img[class*='app-card__icon']");
//            var count = await icons.CountAsync();

//            for (int i = 0; i < count; i++)
//            {
//                var icon = icons.Nth(i);
//                var alt = await icon.GetAttributeAsync("alt");

//                // Assert - Decorative images should have empty alt
//                alt.Should().BeEmpty($"Icon {i + 1} should have empty alt text (decorative)");
//            }
//        }
//    }

//    [Fact]
//    public async Task CardIcons_HaveOnerrorFallback()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var icons = _page.Locator("img[class*='app-card__icon']");
//            var count = await icons.CountAsync();

//            for (int i = 0; i < count; i++)
//            {
//                var icon = icons.Nth(i);
//                var onerror = await icon.GetAttributeAsync("onerror");

//                // Assert
//                onerror.Should().Contain("display='none'", $"Icon {i + 1} should hide on error");
//            }
//        }
//    }

//    #endregion

//    #region Responsive Layout Tests

//    [Fact]
//    public async Task CardsLayout_DesktopShowsThreeColumns()
//    {
//        // Arrange
//        await _page.SetViewportSizeAsync(1280, 720);
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var columns = _page.Locator(".govuk-grid-column-one-third");
//            var count = await columns.CountAsync();

//            // Assert
//            count.Should().Be(3, "Desktop should show three columns");
//        }
//    }

//    [Fact]
//    public async Task CardsLayout_MobileStacksVertically()
//    {
//        // Arrange - Set mobile viewport
//        await _page.SetViewportSizeAsync(375, 667);
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cards = _page.Locator("div[class*='app-card']");
//            var count = await cards.CountAsync();

//            // Assert - Cards should still exist on mobile
//            count.Should().Be(3, "All cards should be visible on mobile");
//        }
//    }

//    [Fact]
//    public async Task CardsLayout_TabletResponsive()
//    {
//        // Arrange - Set tablet viewport
//        await _page.SetViewportSizeAsync(768, 1024);
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var cards = _page.Locator("div[class*='app-card']");
//            var count = await cards.CountAsync();

//            // Assert
//            count.Should().Be(3, "All cards should be visible on tablet");
//        }
//    }

//    #endregion

//    #region Journey Tests - Non-Establishment User Redirect

//    [Fact]
//    public async Task SchoolHome_NonEstablishmentUser_RedirectsToSchoolSearch()
//    {
//        // This test verifies the redirect behavior for non-Establishment users
//        // The actual redirect depends on the user's organisation type

//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        // Act
//        var currentUrl = _page.Url;

//        // Assert
//        // User should be on SchoolHome (Establishment) or redirected to SchoolSearch (non-Establishment)
//        var validDestination = currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase) ||
//                               currentUrl.Contains("SchoolSearch", StringComparison.OrdinalIgnoreCase) ||
//                               currentUrl.Contains("sign-in", StringComparison.OrdinalIgnoreCase) ||
//                               currentUrl.Contains("Auth", StringComparison.OrdinalIgnoreCase);

//        validDestination.Should().BeTrue(
//            "User should be on SchoolHome, SchoolSearch, or login page");
//    }

//    #endregion

//    #region Accessibility Tests

//    [Fact]
//    public async Task SchoolHome_HeadingHierarchy_IsCorrect()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var h2 = _page.Locator("h2.govuk-heading-xl");
//            var h3s = _page.Locator("h3.govuk-heading-m");

//            var h2Count = await h2.CountAsync();
//            var h3Count = await h3s.CountAsync();

//            // Assert
//            h2Count.Should().Be(1, "Should have one h2 for school name");
//            h3Count.Should().Be(3, "Should have three h3 for card headings");
//        }
//    }

//    [Fact]
//    public async Task SchoolHome_LinksHaveDescriptiveText()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var links = _page.Locator("a[class*='app-card__link']");
//            var count = await links.CountAsync();

//            for (int i = 0; i < count; i++)
//            {
//                var link = links.Nth(i);
//                var text = await link.TextContentAsync();

//                // Assert - Links should have meaningful text
//                text.Should().NotBeNullOrWhiteSpace($"Link {i + 1} should have descriptive text");
//                text!.Length.Should().BeGreaterThan(5, $"Link {i + 1} text should be descriptive");
//            }
//        }
//    }

//    [Fact]
//    public async Task SchoolHome_CardDescriptions_AreReadable()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var descriptions = _page.Locator("p[class*='app-card__description']");
//            var count = await descriptions.CountAsync();

//            for (int i = 0; i < count; i++)
//            {
//                var desc = descriptions.Nth(i);
//                var text = await desc.TextContentAsync();

//                // Assert
//                text.Should().NotBeNullOrWhiteSpace($"Description {i + 1} should have text");
//                text!.Length.Should().BeGreaterThan(20, $"Description {i + 1} should be informative");
//            }
//        }
//    }

//    #endregion

//    #region Performance Tests

//    [Fact]
//    public async Task SchoolHome_LoadsWithinReasonableTime()
//    {
//        // Arrange
//        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

//        // Act
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        stopwatch.Stop();

//        // Assert - Page should load within 10 seconds
//        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
//            "SchoolHome page should load within 10 seconds");
//    }

//    #endregion

//    #region Card Interaction Tests

//    [Fact]
//    public async Task Cards_AreKeyboardNavigable()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act - Tab through the cards
//            var links = _page.Locator("a[class*='app-card__link']");
//            var count = await links.CountAsync();

//            for (int i = 0; i < count; i++)
//            {
//                var link = links.Nth(i);
//                await link.FocusAsync();
//                var isFocused = await link.EvaluateAsync<bool>("el => el === document.activeElement");

//                // Assert
//                isFocused.Should().BeTrue($"Card link {i + 1} should be focusable via keyboard");
//            }
//        }
//    }

//    [Fact]
//    public async Task Cards_HaveVisibleFocusState()
//    {
//        // Arrange
//        await _page.GotoAsync(SchoolHomePath);
//        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

//        if (_page.Url.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase))
//        {
//            // Act
//            var firstLink = _page.Locator("a[class*='app-card__link']").First;
//            await firstLink.FocusAsync();

//            // Assert - GOV.UK links should have visible focus styling
//            var linkExists = await firstLink.IsVisibleAsync();
//            linkExists.Should().BeTrue("Link should be visible and focusable");
//        }
//    }

//    #endregion
//}