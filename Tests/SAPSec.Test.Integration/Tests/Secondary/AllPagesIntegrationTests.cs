using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class AllPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private static readonly PageTestCase[] SecondaryPages = [
        new(Routes.SecondarySchool("100001").Overview, "Test School 1", NavigationText: "Overview"),
        new(Routes.SecondarySchool("100001").KS4HeadlineMeasures, "KS4 headline performance measures", NavigationText: "KS4 headline measures"),
        new(Routes.SecondarySchool("100001").KS4CoreSubjects, "KS4 core subject GCSE results", NavigationText: "KS4 core subjects"),
        new(Routes.SecondarySchool("100001").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.SecondarySchool("100001").ViewSimilarSchools, "View similar schools"),
        new(Routes.SecondarySchool("100001").SchoolDetails, "School details"),
        new(Routes.SecondarySchool("100001").WhatIsASimilarSchool, "What is a similar school?"),
        new(Routes.SecondarySchool("100001").RiseResources, "RISE resources"),
        new(Routes.SecondarySchool("100001").Comparison("100002").Similarity, "How these schools compare", NavigationText: "Similarity"),
        new(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, "KS4 headline performance measures", NavigationText: "KS4 headline measures"),
        new(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, "KS4 core subject GCSE results", NavigationText: "KS4 core subjects"),
        new(Routes.SecondarySchool("100001").Comparison("100002").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.SecondarySchool("100001").Comparison("100002").SchoolDetails, "School details", NavigationText: "School details")
    ];

    private static readonly Regex ComparisonPage = new Regex(Routes.SecondarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OverviewPage = new Regex(Routes.SecondarySchool(@"\d{6}").Overview, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.SimilarSchoolsSecondaryRepository.SetupValues(
            Build.SecondaryValues(["100001", "100002", "100003"]));

        return base.InitializeAsync();
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

        if (path.StartsWith(Routes.SecondarySchool("100001").Comparison("100002").BasePath))
        {
            navigationItems.Should().SatisfyRespectively(
                n => n.ShouldLinkTo("Home", Routes.FindASchool()),
                n => n.ShouldLinkTo("View similar schools", Routes.SecondarySchool("100001").ViewSimilarSchools));
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

        var navigationAssertions = SecondaryPages
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

        var navigationAssertions = SecondaryPages
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
        foreach (var page in SecondaryPages)
        {
            data.Add(page.Path);
        }

        return data;
    }

    public static TheoryData<string, string> NonComparisonPages()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in SecondaryPages)
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
        foreach (var page in SecondaryPages)
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
        foreach (var page in SecondaryPages)
        {
            data.Add(page.Path, page.Heading);
        }

        return data;
    }

    private record PageTestCase(string Path, string Heading, string? NavigationText = null);
}
