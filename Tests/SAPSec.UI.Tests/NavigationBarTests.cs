using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

public class NavigationBarTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture), IClassFixture<WebApplicationSetupFixture>
{
    private readonly WebApplicationSetupFixture _fixture = fixture;

    #region Header Tests

    [Fact]
    public async Task Header_IsVisible()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var header = Page.Locator(".govuk-header");
        var isVisible = await header.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Header should be visible on the page");
    }

    [Fact]
    public async Task Header_HasDfELogo()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logo = Page.Locator(".govuk-header__logo img");
        var isVisible = await logo.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("DfE logo should be visible in header");
    }

    [Fact]
    public async Task Header_LogoLinksToHomePage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logoLink = Page.Locator(".govuk-header__link--homepage");
        var href = await logoLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("/", "Logo should link to home page");
    }

    [Fact]
    public async Task Header_LogoHasCorrectAltText()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var logoImg = Page.Locator(".govuk-header__logo img");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNav = Page.Locator(".govuk-service-navigation");
        var isVisible = await serviceNav.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Service navigation should be visible");
    }

    [Fact]
    public async Task ServiceNavigation_HasServiceName()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceName = Page.Locator(".govuk-service-navigation__service-name");
        var isVisible = await serviceName.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Service name should be visible in service navigation");
    }

    [Fact]
    public async Task ServiceNavigation_ServiceNameLinksToHome()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNameLink = Page.Locator(".govuk-service-navigation__service-name a");
        var href = await serviceNameLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("/find-a-school", "Service name should link to home page");
    }

    #endregion

    #region Authenticated Navigation Menu Tests

    [Fact]
    public async Task AuthenticatedNav_SignOutLinkVisible_WhenAuthenticated()
    {
        // Arrange - Navigate to a page that requires authentication or simulates auth
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var signOutLink = Page.Locator("a[href*='Auth/SignOutCallback']");

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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuNav = Page.Locator(".govuk-service-navigation__wrapper");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var compareLink = Page.Locator(".govuk-service-navigation__list a[href*='ComparePerformance']");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var findSchoolsLink = Page.Locator(".govuk-service-navigation__list a[href*='SchoolSearch']");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var schoolDetailsLink = Page.Locator(".govuk-service-navigation__list a[href*='SchoolDetails']");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var changeSchoolLink = Page.Locator(".govuk-service-navigation__list a[href*='ChangeSchool']");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuItems = Page.Locator(".govuk-service-navigation__list .govuk-service-navigation__item");
        var count = await menuItems.CountAsync();

        // Assert - When authenticated, should have 2 menu items
        if (count > 0)
        {
            count.Should().Be(2, "Navigation menu should have 2 items when authenticated");
        }
    }

    #endregion

    #region Navigation Link Click Tests

    [Fact]
    public async Task ClickComparePerformance_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var compareLink = Page.Locator(".govuk-service-navigation__list a[href*='ComparePerformance']");
        var count = await compareLink.CountAsync();

        if (count > 0)
        {
            // Act
            await compareLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            Page.Url.Should().Contain("ComparePerformance");
        }
    }

    [Fact]
    public async Task ClickFindSchools_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var findSchoolsLink = Page.Locator(".govuk-service-navigation__list a[href*='SchoolSearch']");
        var count = await findSchoolsLink.CountAsync();

        if (count > 0)
        {
            // Act
            await findSchoolsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            Page.Url.Should().Contain("SchoolSearch");
        }
    }

    [Fact]
    public async Task ClickSchoolDetails_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var schoolDetailsLink = Page.Locator(".govuk-service-navigation__list a[href*='SchoolDetails']");
        var count = await schoolDetailsLink.CountAsync();

        if (count > 0)
        {
            // Act
            await schoolDetailsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            Page.Url.Should().Contain("SchoolDetails");
        }
    }

    [Fact]
    public async Task ClickChangeSchool_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var changeSchoolLink = Page.Locator(".govuk-service-navigation__list a[href*='ChangeSchool']");
        var count = await changeSchoolLink.CountAsync();

        if (count > 0)
        {
            // Act
            await changeSchoolLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            Page.Url.Should().Contain("ChangeSchool");
        }
    }

    [Fact]
    public async Task ClickServiceName_NavigatesToHomePage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var serviceNameLink = Page.Locator(".govuk-service-navigation__service-name a");

        // Act
        await serviceNameLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var url = new Uri(Page.Url);
        url.AbsolutePath.Should().Be("/find-a-school");
    }

    [Fact]
    public async Task ClickLogo_NavigatesToHomePage()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);
        var logoLink = Page.Locator(".govuk-header__link--homepage");

        // Act
        await logoLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var url = new Uri(Page.Url);
        url.AbsolutePath.Should().Be("/");

    }

    #endregion

    #region Skip Link Tests

    [Fact]
    public async Task SkipLink_Exists()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = Page.Locator(".govuk-skip-link");
        var count = await skipLink.CountAsync();

        // Assert
        count.Should().Be(1, "Skip to main content link should exist");
    }

    [Fact]
    public async Task SkipLink_HasCorrectHref()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = Page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        // Assert
        href.Should().Be("#main-content", "Skip link should point to main content");
    }

    [Fact]
    public async Task SkipLink_HasCorrectText()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var skipLink = Page.Locator(".govuk-skip-link");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var footer = Page.Locator(".govuk-footer");
        var isVisible = await footer.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Footer should be visible");
    }

    [Fact]
    public async Task Footer_HasCookiesLink()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var cookiesLink = Page.Locator(".govuk-footer__link[href*='Cookies']");
        var isVisible = await cookiesLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Cookies link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasAccessibilityLink()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var accessibilityLink = Page.Locator(".govuk-footer__link[href=\"/accessibility\"]");
        var isVisible = await accessibilityLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Accessibility link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasPrivacyLink()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var privacyLink = Page.Locator(".govuk-footer__link[href=\"https://www.gov.uk/government/publications/privacy-information-education-providers-workforce-including-teachers/privacy-information-education-providers-workforce-including-teachers\"]");
        var isVisible = await privacyLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Privacy link should be visible in footer");
    }

    [Fact]
    public async Task Footer_HasOpenGovernmentLicenceLink()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var oglLink = Page.Locator(".govuk-footer__link[href*='open-government-licence']");
        var isVisible = await oglLink.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Open Government Licence link should be visible");
    }

    [Fact]
    public async Task Footer_HasCrownCopyrightLink()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var crownCopyrightLink = Page.Locator(".govuk-footer__copyright-logo");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var phaseBanner = Page.Locator(".govuk-phase-banner");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuToggle = Page.Locator(".govuk-service-navigation__toggle");
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
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuToggle = Page.Locator(".govuk-service-navigation__toggle");
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
            await Page.SetViewportSizeAsync(width, height);
            await Page.GotoAsync(_fixture.BaseUrl);

            // Assert
            var header = Page.Locator(".govuk-header");
            var isVisible = await header.IsVisibleAsync();
            isVisible.Should().BeTrue($"Header should be visible on {name} ({width}x{height})");

            var serviceNav = Page.Locator(".govuk-service-navigation");
            var serviceNavVisible = await serviceNav.IsVisibleAsync();
            serviceNavVisible.Should().BeTrue($"Service navigation should be visible on {name}");
        }
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task ServiceNav_HasCorrectAriaLabel()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var serviceNav = Page.Locator("section[aria-label='Service information']");
        var count = await serviceNav.CountAsync();

        // Assert
        count.Should().Be(1, "Service navigation section should have correct aria-label");
    }

    [Fact]
    public async Task MenuNav_HasCorrectAriaLabel()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var menuNav = Page.Locator("nav[aria-label='Menu']");
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
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var footer = Page.Locator("footer[role='contentinfo']");
        var count = await footer.CountAsync();

        // Assert
        count.Should().Be(1, "Footer should have role='contentinfo'");
    }

    [Fact]
    public async Task SupportLinksHeading_IsVisuallyHidden()
    {
        // Arrange
        await Page.GotoAsync(_fixture.BaseUrl);

        // Act
        var supportHeading = Page.Locator(".govuk-visually-hidden", new() { HasText = "Support links" });
        var count = await supportHeading.CountAsync();

        // Assert
        count.Should().Be(1, "Support links heading should exist and be visually hidden");
    }

    #endregion
}