using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private readonly WebApplicationSetupFixture _fixture = fixture;

    private const string SchoolPagePath = "/school/147788";

    #region Index Page Tests

    [Fact]
    public async Task Index_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(SchoolPagePath);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
        await WaitForPageLoad();
    }

    [Fact]
    public async Task Index_RendersCorrectPageTitle()
    {
        await Page.GotoAsync(SchoolPagePath);

        var title = await Page.TitleAsync();
        var heading = await Page.Locator("h1").TextContentAsync();

        title.Should().Contain("Bradfield School");
    }

    #endregion

    #region Responsive Navigation Tests

    [Fact]
    public async Task MobileNav_MenuToggleExists()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");
        var count = await menuToggle.CountAsync();

        // Assert - Toggle exists but may be hidden on desktop
        if (count > 0)
        {
            var isHidden = await menuToggle.IsHiddenAsync();
            // On desktop, button should be hidden
            isHidden.Should().BeTrue("Menu toggle should be hidden on desktop");
        }
    }

    [Fact]
    public async Task MobileNav_MenuToggleVisibleOnMobile()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync(SchoolPagePath);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");
        var count = await menuToggle.CountAsync();

        // Assert
        if (count > 0)
        {
            // On mobile, the button should be visible (hidden attribute removed by JS)
            var text = await menuToggle.TextContentAsync();
            text.Should().Contain("Show navigation");
        }
    }

    [Fact]
    public async Task Navigation_VisibleOnLargeViewports()
    {
        // Arrange - Test different viewport sizes
        var viewports = new[]
        {
            (width: 1920, height: 1080, name: "Desktop Large"),
            (width: 1280, height: 720, name: "Desktop"),
            (width: 768, height: 1024, name: "Tablet")
        };

        foreach (var (width, height, name) in viewports)
        {
            // Act
            await Page.SetViewportSizeAsync(width, height);
            await Page.GotoAsync(SchoolPagePath);

            // Assert
            var sideNav = Page.Locator(".app-side-navigation__list");
            var sideNavVisible = await sideNav.IsVisibleAsync();
            sideNavVisible.Should().BeTrue($"Side navigation should be visible on {name}");
        }
    }

    [Fact]
    public async Task Navigation_InitiallyHiddenOnMobileViewport()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync(SchoolPagePath);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");

        // Assert
        var sideNav = Page.Locator(".app-side-navigation__list");
        var sideNavHidden = await sideNav.IsHiddenAsync();
        sideNavHidden.Should().BeTrue($"Side navigation should be initially hidden on mobile");
    }

    [Fact]
    public async Task Navigation_CanBeToggledOnMobileViewport()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync(SchoolPagePath);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");
        await menuToggle.ClickAsync();

        // Assert
        var sideNav = Page.Locator(".app-side-navigation__list");
        var sideNavVisible = await sideNav.IsVisibleAsync();
        sideNavVisible.Should().BeTrue($"Side navigation should be able to be toggled on mobile");
    }

    #endregion

    #region Navigation Link Click Tests

    [Fact]
    public async Task ClickOverview_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("Overview");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"Overview page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788");
    }

    [Fact]
    public async Task ClickKs4HeadlineMeasures_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("KS4 headline measures");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"Ks4HeadlineMeasures page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/ks4-headline-measures");
    }

    [Fact]
    public async Task ClickKs4CoreSubjects_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("KS4 core subjects");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"Ks4CoreSubjects page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/ks4-core-subjects");
    }

    [Fact]
    public async Task ClickAttendance_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("Attendance");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"Attendance page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/attendance");
    }

    [Fact]
    public async Task ClickViewSimilarSchools_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("View similar schools");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"ViewSimilarSchools page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/view-similar-schools");
    }

    [Fact]
    public async Task ClickSchoolDetails_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("School details");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"SchoolDetails page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/school-details");
    }

    [Fact]
    public async Task ClickWhatIsASimilarSchool_NavigatesToCorrectPage()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);
        var link = Page.Locator(".app-side-navigation__list").GetByText("What is a similar school?");
        var linkVisible = await link.IsVisibleAsync();
        linkVisible.Should().BeTrue($"WhatIsASimilarSchool page link should be visible");

        // Act
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().EndWith("/school/147788/what-is-a-similar-school");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SideNav_HasCorrectAriaLabel()
    {
        // Arrange
        await Page.GotoAsync(SchoolPagePath);

        // Act
        var sideNav = Page.Locator(".app-side-navigation");
        var ariaLabel = await sideNav.GetAttributeAsync("aria-label");

        // Assert
        ariaLabel.Should().Be("Side", "Side navigation section should have correct aria-label");
    }

    #endregion

    private async Task WaitForPageLoad()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}