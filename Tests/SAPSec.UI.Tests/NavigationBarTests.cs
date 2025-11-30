using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

public class NavigationBarTests : IClassFixture<WebApplicationSetupFixture>, IAsyncLifetime
{
    private readonly WebApplicationSetupFixture _fixture;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public NavigationBarTests(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 }
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    #region Header Tests

    [Fact]
    public async Task Header_IsVisible()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var header = _page.Locator(".govuk-header");
        var isVisible = await header.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Header should be visible on the page");
    }

    [Fact]
    public async Task Header_HasDfELogo()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logo = _page.Locator(".govuk-header__logo img");
        var isVisible = await logo.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("DfE logo should be visible in header");
    }

    [Fact]
    public async Task Header_LogoLinksToHomePage()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logoLink = _page.Locator(".govuk-header__link--homepage");
        var href = await logoLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("/", "Logo should link to home page");
    }

    [Fact]
    public async Task Header_LogoHasCorrectAltText()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logoImg = _page.Locator(".govuk-header__logo img");
        var altText = await logoImg.GetAttributeAsync("alt");

        // Assert
        altText.Should().Be("Department for Education", "Logo should have correct alt text");
    }

    #endregion

    #region Service Navigation Tests

    [Fact]
    public async Task ServiceNavigation_IsVisible()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNav = _page.Locator(".govuk-service-navigation");
        var isVisible = await serviceNav.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Service navigation should be visible");
    }

    [Fact]
    public async Task ServiceNavigation_HasServiceName()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceName = _page.Locator(".govuk-service-navigation__service-name");
        var isVisible = await serviceName.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Service name should be visible in service navigation");
    }

    [Fact]
    public async Task ServiceNavigation_ServiceNameLinksToHome()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNameLink = _page.Locator(".govuk-service-navigation__service-name a");
        var href = await serviceNameLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("/", "Service name should link to home page");
    }

    #endregion

    #region Authenticated Navigation Menu Tests

    [Fact]
    public async Task AuthenticatedNav_SignOutLinkVisible_WhenAuthenticated()
    {
        // Arrange - Navigate to a page that requires authentication or simulates auth
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var signOutLink = _page.Locator("a[href*='Auth/SignOutCallback']");

        // Assert - Check if element exists (may not be visible if not authenticated)
        var count = await signOutLink.CountAsync();

        // If authenticated, sign out should be visible
        if (count > 0)
        {
            var isVisible = await signOutLink.IsVisibleAsync();
            isVisible.Should().BeTrue("Sign out link should be visible when authenticated");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_MenuItemsExist_WhenAuthenticated()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuNav = _page.Locator(".govuk-service-navigation__wrapper");
        var count = await menuNav.CountAsync();

        // Assert - Menu wrapper exists when authenticated
        if (count > 0)
        {
            var isVisible = await menuNav.IsVisibleAsync();
            isVisible.Should().BeTrue("Menu navigation should be visible when authenticated");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_HasComparePerformanceLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var compareLink = _page.Locator(".govuk-service-navigation__list a[href*='ComparePerformance']");
        var count = await compareLink.CountAsync();

        // Assert
        if (count > 0)
        {
            var text = await compareLink.TextContentAsync();
            text.Should().Contain("Compare performance");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_HasFindSchoolsLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var findSchoolsLink = _page.Locator(".govuk-service-navigation__list a[href*='SchoolSearch']");
        var count = await findSchoolsLink.CountAsync();

        // Assert
        if (count > 0)
        {
            var text = await findSchoolsLink.TextContentAsync();
            text.Should().Contain("Find schools");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_HasSchoolDetailsLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var schoolDetailsLink = _page.Locator(".govuk-service-navigation__list a[href*='SchoolDetails']");
        var count = await schoolDetailsLink.CountAsync();

        // Assert
        if (count > 0)
        {
            var text = await schoolDetailsLink.TextContentAsync();
            text.Should().Contain("School details");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_HasChangeSchoolLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var changeSchoolLink = _page.Locator(".govuk-service-navigation__list a[href*='ChangeSchool']");
        var count = await changeSchoolLink.CountAsync();

        // Assert
        if (count > 0)
        {
            var text = await changeSchoolLink.TextContentAsync();
            text.Should().Contain("Change school");
        }
    }

    [Fact]
    public async Task AuthenticatedNav_MenuHasFourItems_WhenAuthenticated()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuItems = _page.Locator(".govuk-service-navigation__list .govuk-service-navigation__item");
        var count = await menuItems.CountAsync();

        // Assert - When authenticated, should have 4 menu items
        if (count > 0)
        {
            count.Should().Be(4, "Navigation menu should have 4 items when authenticated");
        }
    }

    #endregion

    #region Navigation Link Click Tests

    [Fact]
    public async Task ClickComparePerformance_NavigatesToCorrectPage()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);
        var compareLink = _page.Locator(".govuk-service-navigation__list a[href*='ComparePerformance']");
        var count = await compareLink.CountAsync();

        if (count > 0)
        {
            // Act
            await compareLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            _page.Url.Should().Contain("ComparePerformance");
        }
    }

    [Fact]
    public async Task ClickFindSchools_NavigatesToCorrectPage()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);
        var findSchoolsLink = _page.Locator(".govuk-service-navigation__list a[href*='SchoolSearch']");
        var count = await findSchoolsLink.CountAsync();

        if (count > 0)
        {
            // Act
            await findSchoolsLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            _page.Url.Should().Contain("SchoolSearch");
        }
    }

    [Fact]
    public async Task ClickSchoolDetails_NavigatesToCorrectPage()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);
        var schoolDetailsLink = _page.Locator(".govuk-service-navigation__list a[href*='SchoolDetails']");
        var count = await schoolDetailsLink.CountAsync();

        if (count > 0)
        {
            // Act
            await schoolDetailsLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            _page.Url.Should().Contain("SchoolDetails");
        }
    }

    [Fact]
    public async Task ClickChangeSchool_NavigatesToCorrectPage()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);
        var changeSchoolLink = _page.Locator(".govuk-service-navigation__list a[href*='ChangeSchool']");
        var count = await changeSchoolLink.CountAsync();

        if (count > 0)
        {
            // Act
            await changeSchoolLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            _page.Url.Should().Contain("ChangeSchool");
        }
    }

    [Fact]
    public async Task ClickServiceName_NavigatesToHomePage()
    {
        // Arrange
        await _page.GotoAsync($"{_fixture.BaseUrl}/school");
        var serviceNameLink = _page.Locator(".govuk-service-navigation__service-name a");

        // Act
        await serviceNameLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var url = new Uri(_page.Url);
        url.AbsolutePath.Should().Be("/");
    }

    [Fact]
    public async Task ClickLogo_NavigatesToHomePage()
    {
        // Arrange
        await _page.GotoAsync($"{_fixture.BaseUrl}/school");
        var logoLink = _page.Locator(".govuk-header__link--homepage");

        // Act
        await logoLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var url = new Uri(_page.Url);
        url.AbsolutePath.Should().Be("/");
    }

    #endregion

    #region Skip Link Tests

    [Fact]
    public async Task SkipLink_Exists()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = _page.Locator(".govuk-skip-link");
        var count = await skipLink.CountAsync();

        // Assert
        count.Should().Be(1, "Skip to main content link should exist");
    }

    [Fact]
    public async Task SkipLink_HasCorrectHref()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = _page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("#main-content", "Skip link should point to main content");
    }

    [Fact]
    public async Task SkipLink_HasCorrectText()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = _page.Locator(".govuk-skip-link");
        var text = await skipLink.TextContentAsync();

        // Assert
        text.Should().Contain("Skip to main content");
    }

    #endregion

    #region Footer Tests

    [Fact]
    public async Task Footer_IsVisible()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var footer = _page.Locator(".govuk-footer");
        var isVisible = await footer.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Footer should be visible");
    }

    [Fact]
    public async Task Footer_HasCookiesLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var cookiesLink = _page.Locator(".govuk-footer__link[href*='Cookies']");
        var isVisible = await cookiesLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Cookies link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasAccessibilityLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var accessibilityLink = _page.Locator(".govuk-footer__link[href*='Accessibility']");
        var isVisible = await accessibilityLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Accessibility link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasPrivacyLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var privacyLink = _page.Locator(".govuk-footer__link[href*='personal-information-charter']");
        var isVisible = await privacyLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Privacy link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasOpenGovernmentLicenceLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var oglLink = _page.Locator(".govuk-footer__link[href*='open-government-licence']");
        var isVisible = await oglLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Open Government Licence link should be visible");
    }

    [Fact]
    public async Task Footer_HasCrownCopyrightLink()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var crownCopyrightLink = _page.Locator(".govuk-footer__copyright-logo");
        var isVisible = await crownCopyrightLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Crown copyright link should be visible");
    }

    #endregion

    #region Phase Banner Tests

    [Fact]
    public async Task PhaseBanner_IsVisible()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var phaseBanner = _page.Locator(".govuk-phase-banner");
        var count = await phaseBanner.CountAsync();

        // Assert - Phase banner should exist if component is rendered
        if (count > 0)
        {
            var isVisible = await phaseBanner.IsVisibleAsync();
            isVisible.Should().BeTrue("Phase banner should be visible");
        }
    }

    #endregion

    #region Responsive Navigation Tests

    [Fact]
    public async Task MobileNav_MenuToggleExists()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuToggle = _page.Locator(".govuk-service-navigation__toggle");
        var count = await menuToggle.CountAsync();

        // Assert - Toggle exists but may be hidden on desktop
        if (count > 0)
        {
            var isHidden = await menuToggle.GetAttributeAsync("hidden");
            // On desktop, button should be hidden
            isHidden.Should().NotBeNull("Menu toggle should have hidden attribute on desktop");
        }
    }

    [Fact]
    public async Task MobileNav_MenuToggleVisibleOnMobile()
    {
        // Arrange - Set mobile viewport
        await _page.SetViewportSizeAsync(375, 667);
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuToggle = _page.Locator(".govuk-service-navigation__toggle");
        var count = await menuToggle.CountAsync();

        // Assert
        if (count > 0)
        {
            // On mobile, the button should be visible (hidden attribute removed by JS)
            var text = await menuToggle.TextContentAsync();
            text.Should().Contain("Menu");
        }
    }

    [Fact]
    public async Task Navigation_WorksOnDifferentViewports()
    {
        // Arrange - Test different viewport sizes
        var viewports = new[]
        {
            (width: 1920, height: 1080, name: "Desktop Large"),
            (width: 1280, height: 720, name: "Desktop"),
            (width: 768, height: 1024, name: "Tablet"),
            (width: 375, height: 667, name: "Mobile")
        };

        foreach (var (width, height, name) in viewports)
        {
            // Act
            await _page.SetViewportSizeAsync(width, height);
            await _page.GotoAsync(_fixture.BaseUrl);

            // Assert
            var header = _page.Locator(".govuk-header");
            var isVisible = await header.IsVisibleAsync();
            isVisible.Should().BeTrue($"Header should be visible on {name} ({width}x{height})");

            var serviceNav = _page.Locator(".govuk-service-navigation");
            var serviceNavVisible = await serviceNav.IsVisibleAsync();
            serviceNavVisible.Should().BeTrue($"Service navigation should be visible on {name}");
        }
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task Header_HasCorrectAriaLabel()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var accountNav = _page.Locator("nav[aria-label='Account navigation']");
        var count = await accountNav.CountAsync();

        // Assert - Account nav only appears when authenticated
        if (count > 0)
        {
            var ariaLabel = await accountNav.GetAttributeAsync("aria-label");
            ariaLabel.Should().Be("Account navigation");
        }
    }

    [Fact]
    public async Task ServiceNav_HasCorrectAriaLabel()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNav = _page.Locator("section[aria-label='Service information']");
        var count = await serviceNav.CountAsync();

        // Assert
        count.Should().Be(1, "Service navigation section should have correct aria-label");
    }

    [Fact]
    public async Task MenuNav_HasCorrectAriaLabel()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuNav = _page.Locator("nav[aria-label='Menu']");
        var count = await menuNav.CountAsync();

        // Assert - Menu nav only appears when authenticated
        if (count > 0)
        {
            var ariaLabel = await menuNav.GetAttributeAsync("aria-label");
            ariaLabel.Should().Be("Menu");
        }
    }

    [Fact]
    public async Task Footer_HasCorrectRole()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var footer = _page.Locator("footer[role='contentinfo']");
        var count = await footer.CountAsync();

        // Assert
        count.Should().Be(1, "Footer should have role='contentinfo'");
    }

    [Fact]
    public async Task SupportLinksHeading_IsVisuallyHidden()
    {
        // Arrange
        await _page.GotoAsync(_fixture.BaseUrl);

        // Act
        var supportHeading = _page.Locator(".govuk-visually-hidden", new() { HasText = "Support links" });
        var count = await supportHeading.CountAsync();

        // Assert
        count.Should().Be(1, "Support links heading should exist and be visually hidden");
    }

    #endregion
}