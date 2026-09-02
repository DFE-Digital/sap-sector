using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class AllPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private static readonly PageTestCase[] SecondaryPages = [
        new(Routes.SecondarySchool("100001").Overview, "Test School 1", NavigationText: "Overview", IsOverviewPage: true),
        new(Routes.SecondarySchool("100001").KS4HeadlineMeasures, "KS4 headline performance measures", NavigationText: "KS4 headline measures"),
        new(Routes.SecondarySchool("100001").KS4CoreSubjects, "KS4 core subject GCSE results", NavigationText: "KS4 core subjects"),
        new(Routes.SecondarySchool("100001").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.SecondarySchool("100001").ViewSimilarSchools, "View similar schools"),
        new(Routes.SecondarySchool("100001").SchoolDetails, "School details"),
        new(Routes.SecondarySchool("100001").WhatIsASimilarSchool, "What is a similar school?"),
        new(Routes.SecondarySchool("100001").Comparison("100002").Similarity, "Test School 2", IsInNavigation: false),
        new(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, "Test School 2", IsInNavigation: false),
        new(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, "Test School 2", IsInNavigation: false),
        new(Routes.SecondarySchool("100001").Comparison("100002").Attendance, "Test School 2", IsInNavigation: false),
        new(Routes.SecondarySchool("100001").Comparison("100002").SchoolDetails, "Test School 2", IsInNavigation: false)
    ];

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.SimilarSchoolsSecondaryRepository.SetupValues(
            Build.SecondaryValues("100001", "100002", "100003"));

        return base.InitializeAsync();
    }

    [Theory]
    [MemberData(nameof(AllPagesWithPageHeadings))]
    public async Task AllPages_Headings(string path, string expectedHeading, bool isOverviewPage)
    {
        var page = await Fixture.RequestPageAsync(path);
        page.Title.Should().Be($"{expectedHeading} - Get school improvement insights - GOV.UK");

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading.TrimmedTextContent().Should().Be(expectedHeading);

        if (!isOverviewPage)
        {
            var caption = page.QuerySelector(".govuk-caption-xl");
            caption.Should().NotBeNull();
            caption.TrimmedTextContent().Should().Be("Test School 1");
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
    [MemberData(nameof(AllPagesWithSideNavigation))]
    public async Task AllPages_Navigation_ShowsLinksInCorrectOrder(string path)
    {
        var page = await Fixture.RequestPageAsync(path);

        var navigationItems = page.QuerySelectorAll(".app-side-navigation__item a");

        var navigationAssertions = SecondaryPages
            .Where(p => p.IsInNavigation)
            .Select(p => new Action<IElement>(n => n.ShouldLinkTo(p.NavigationText ?? p.Heading, p.Path)))
            .ToArray();

        navigationItems.Should().SatisfyRespectively(navigationAssertions);
    }

    [Theory]
    [MemberData(nameof(AllPagesInNavigation))]
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

    [Fact]
    public async Task OverviewPage_ContainsWhatIsASimilarSchoolLink()
    {
        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Overview);

        var link = page.QuerySelector(".app-body-container-with-side-navigation a");
        link.Should().NotBeNull();
        link.GetAttribute("href").Should().Be(Routes.SecondarySchool("100001").WhatIsASimilarSchool);
    }

    [Fact]
    public async Task WhatIsASimilarSchoolPage_ContainsViewSimilarSchoolsLink()
    {
        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").WhatIsASimilarSchool);

        var links = page.QuerySelectorAll(".app-body-container-with-side-navigation a");
        links.Should().Contain(l => l.GetAttribute("href") == Routes.SecondarySchool("100001").ViewSimilarSchools);
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

    public static TheoryData<string> AllPagesWithSideNavigation()
    {
        var data = new TheoryData<string>();
        foreach (var page in SecondaryPages)
        {
            if (page.IsInNavigation)
            {
                data.Add(page.Path);
            }
        }

        return data;
    }

    public static TheoryData<string, string> AllPagesInNavigation()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in SecondaryPages)
        {
            if (page.IsInNavigation)
            {
                data.Add(page.Path, page.NavigationText ?? page.Heading);
            }
        }

        return data;
    }

    public static TheoryData<string, string, bool> AllPagesWithPageHeadings()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (var page in SecondaryPages)
        {
            data.Add(page.Path, page.Heading, page.IsOverviewPage);
        }

        return data;
    }

    private record PageTestCase(string Path, string Heading, string? NavigationText = null, bool IsOverviewPage = false, bool IsInNavigation = true);
}
