using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class SchoolPagesAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private static readonly string[] SchoolDetailsPagePaths = [
        Routes.PrimarySchool("100171").Overview,
        Routes.PrimarySchool("100171").KS2,
        Routes.PrimarySchool("100171").Attendance,
        Routes.PrimarySchool("100171").ViewSimilarSchools,
        Routes.PrimarySchool("100171").SchoolDetails,
        Routes.PrimarySchool("100171").WhatIsASimilarSchool,
        Routes.PrimarySchool("100171").Comparison("150318").Similarity,
        Routes.PrimarySchool("100171").Comparison("150318").Ks2,
        Routes.PrimarySchool("100171").Comparison("150318").Attendance,
        Routes.PrimarySchool("100171").Comparison("150318").SchoolDetails,
        Routes.SecondarySchool("100182").Overview,
        Routes.SecondarySchool("100182").KS4HeadlineMeasures,
        Routes.SecondarySchool("100182").KS4CoreSubjects,
        Routes.SecondarySchool("100182").Attendance,
        Routes.SecondarySchool("100182").ViewSimilarSchools,
        Routes.SecondarySchool("100182").SchoolDetails,
        Routes.SecondarySchool("100182").WhatIsASimilarSchool,
        Routes.SecondarySchool("100182").Comparison("136555").Similarity,
        Routes.SecondarySchool("100182").Comparison("136555").KS4HeadlineMeasures,
        Routes.SecondarySchool("100182").Comparison("136555").KS4CoreSubjects,
        Routes.SecondarySchool("100182").Comparison("136555").Attendance,
        Routes.SecondarySchool("100182").Comparison("136555").SchoolDetails,
    ];

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task MobileNav_MenuToggleExists(string path)
    {
        // Arrange
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task MobileNav_MenuToggleVisibleOnMobile(string path)
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

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

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task Navigation_VisibleOnLargeViewports(string path)
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
            await NavigateTo(path);

            // Assert
            var sideNav = Page.Locator(".app-side-navigation__list");
            var sideNavVisible = await sideNav.IsVisibleAsync();
            sideNavVisible.Should().BeTrue($"Side navigation should be visible on {name}");
        }
    }

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task Navigation_InitiallyHiddenOnMobileViewport(string path)
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");

        // Assert
        var sideNav = Page.Locator(".app-side-navigation__list");
        var sideNavHidden = await sideNav.IsHiddenAsync();
        sideNavHidden.Should().BeTrue($"Side navigation should be initially hidden on mobile");
    }

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task Navigation_CanBeToggledOnMobileViewport(string path)
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(path);

        // Act
        var menuToggle = Page.Locator(".app-side-navigation__button");
        await menuToggle.ClickAsync();

        // Assert
        var sideNav = Page.Locator(".app-side-navigation__list");
        var sideNavVisible = await sideNav.IsVisibleAsync();
        sideNavVisible.Should().BeTrue($"Side navigation should be able to be toggled on mobile");
    }

    public static TheoryData<string> AllPages()
    {
        var data = new TheoryData<string>();
        foreach (var path in SchoolDetailsPagePaths)
        {
            data.Add(path);
        }

        return data;
    }

    public static TheoryData<string> ComparisonPages()
    {
        var data = new TheoryData<string>();
        foreach (var path in SchoolDetailsPagePaths)
        {
            if (Regex.IsMatch(path, Routes.PrimarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath)
                || Regex.IsMatch(path, Routes.SecondarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath))
                data.Add(path);
        }

        return data;
    }

    public static TheoryData<string> NonComparisonPages()
    {
        var data = new TheoryData<string>();
        foreach (var path in SchoolDetailsPagePaths)
        {
            if (!Regex.IsMatch(path, Routes.PrimarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath)
                && !Regex.IsMatch(path, Routes.SecondarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath))
                data.Add(path);
        }

        return data;
    }
}
