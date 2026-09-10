using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class AllPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private static readonly PageTestCase[] PrimaryPages = [
        new(Routes.PrimarySchool("100001").Overview, "Test School 1", NavigationText: "Overview"),
        new(Routes.PrimarySchool("100001").KS2, "KS2 performance measures", NavigationText: "KS2"),
        new(Routes.PrimarySchool("100001").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.PrimarySchool("100001").ViewSimilarSchools, "View similar schools"),
        new(Routes.PrimarySchool("100001").SchoolDetails, "School details"),
        new(Routes.PrimarySchool("100001").WhatIsASimilarSchool, "What is a similar school?"),
        new(Routes.PrimarySchool("100001").RiseResources, "RISE resources"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Similarity, "How these schools compare", NavigationText: "Similarity"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Ks2, "KS2 performance measures", NavigationText: "KS2"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails, "School details", NavigationText: "School details")
    ];

    private static readonly Regex ComparisonPage = new Regex(Routes.PrimarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OverviewPage = new Regex(Routes.PrimarySchool(@"\d{6}").Overview, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            Build.PrimaryValues(["100001", "100002", "100003"]));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_WhenPrimarySchoolsFeatureFlagDisabled_ReturnNotFound(string path)
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnablePrimarySchools, false);

        await Fixture.RequestPageAsync(path, HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AllPagesWithPageHeadings))]
    public async Task AllPages_Headings(string path, string expectedHeading)
    {
        var isComparisonPage = ComparisonPage.IsMatch(path);
        var isOverviewPage = OverviewPage.IsMatch(path);

        var page = await Fixture.RequestPageAsync(path);

        var expectedTitle = isComparisonPage ? "Test School 2" : expectedHeading;
        page.Title.Should().Be($"{expectedTitle} - Get school improvement insights - GOV.UK");

        var h1 = page.QuerySelector("h1.govuk-heading-xl");
        h1.Should().NotBeNull();
        h1.TrimmedTextContent().Should().Be(isComparisonPage ? "Test School 2" : expectedHeading);

        if (!isOverviewPage)
        {
            var caption = page.QuerySelector(".govuk-caption-xl");
            caption.Should().NotBeNull();
            caption.TrimmedTextContent().Should().Be("Test School 1");
        }

        if (isComparisonPage)
        {
            var h2 = page.QuerySelector("h2.govuk-heading-l");
            h2.Should().NotBeNull();
            h2.TrimmedTextContent().Should().Be(expectedHeading);
        }
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task AllPages_Breadcrumbs(string path)
    {
        var page = await Fixture.RequestPageAsync(path);

        var navigationItems = page.QuerySelectorAll(".govuk-breadcrumbs__list-item a");
        if (path.StartsWith(Routes.PrimarySchool("100001").Comparison("100002").BasePath))
        {
            navigationItems.Should().SatisfyRespectively(
                n => n.ShouldLinkTo("Home", Routes.FindASchool()),
                n => n.ShouldLinkTo("View similar schools", Routes.PrimarySchool("100001").ViewSimilarSchools));
        }
        else
        {
            navigationItems.Should().SatisfyRespectively(
                n => n.ShouldLinkTo("Home", Routes.FindASchool()));
        }
    }

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task AllPages_Navigation_ShowsLinksInCorrectOrder(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync(path);

        var navigationItems = page.QuerySelectorAll(".app-side-navigation__item a");

        var hrefs = navigationItems.Cast<IHtmlAnchorElement>().Select(a => a.Href).ToArray();

        var navigationAssertions = PrimaryPages
            .Where(p => !ComparisonPage.IsMatch(p.Path))
            .Select(p => new Action<IElement>(n => n.ShouldLinkTo(p.NavigationText ?? p.Heading, p.Path)))
            .ToArray();

        navigationItems.Should().SatisfyRespectively(navigationAssertions);
    }

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task AllPages_Navigation_ShowsSelectedTabAsActive(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync(path);

        var navigationItem = page.QuerySelector(".app-side-navigation__item--selected");
        navigationItem.Should().NotBeNull();
        navigationItem.TrimmedTextContent().Should().Be(navigationText);

        var link = navigationItem.QuerySelector("a");
        link.Should().NotBeNull();
        link.ClassList.Should().Contain("app-side-navigation__link--selected");
        link.GetAttribute("aria-current").Should().Be("page");
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task AllPages_NotInNavigation_HasBackLink(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync(path);

        var backLink = page.QuerySelector("a.govuk-back-link");
        backLink.Should().NotBeNull();

        var linkText = backLink.TrimmedTextContent();
        linkText.Should().Contain("Back");

        var href = backLink.GetAttribute("href");

        href.Should().NotBeNullOrWhiteSpace("Back link should have an href");
        href!.Should().Contain("view-similar-schools", "Back link should navigate to the similar schools journey");
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task AllPages_NotInNavigation_HasAllTabs(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync(path);

        var tabs = page.ElementsShouldExist("div.govuk-service-navigation.compare-nav a.govuk-service-navigation__link");

        var navigationAssertions = PrimaryPages
            .Where(p => ComparisonPage.IsMatch(p.Path))
            .Select(p => new Action<IElement>(n => n.ShouldLinkTo(p.NavigationText ?? p.Heading, p.Path)))
            .ToArray();

        tabs.Should().SatisfyRespectively(navigationAssertions);
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task AllPages_ActiveTab(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync(path);

        var activeTab = page.ElementShouldExist("li.govuk-service-navigation__item--active a.govuk-service-navigation__link");
        activeTab.TrimmedTextContent().Should().Be(navigationText);

        var ariaCurrent = activeTab.GetAttribute("aria-current");
        ariaCurrent.Should().Be("page", "Active tab should have aria-current='page'");
    }

    public static TheoryData<string> AllPages()
    {
        var data = new TheoryData<string>();
        foreach (var page in PrimaryPages)
        {
            data.Add(page.Path);
        }

        return data;
    }

    public static TheoryData<string, string> NonComparisonPages()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in PrimaryPages)
        {
            if (!ComparisonPage.IsMatch(page.Path))
            {
                data.Add(page.Path, page.NavigationText ?? page.Heading);
            }
        }

        return data;
    }

    public static TheoryData<string, string> ComparisonPages()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in PrimaryPages)
        {
            if (ComparisonPage.IsMatch(page.Path))
            {
                data.Add(page.Path, page.NavigationText ?? page.Heading);
            }
        }

        return data;
    }

    public static TheoryData<string, string> AllPagesWithPageHeadings()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in PrimaryPages)
        {
            data.Add(page.Path, page.Heading);
        }

        return data;
    }

    private record PageTestCase(string Path, string Heading, string? NavigationText = null);
}
