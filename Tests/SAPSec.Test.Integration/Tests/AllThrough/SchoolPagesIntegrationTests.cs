using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.AllThrough;

public class SchoolPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Urn = "100001";

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnableAllThroughSchools);
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnableRiseResources);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task OverviewPage_WithSimilarSchools_ShowsExpectedNavigation()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        AssertNavigation(page, [
            ("Overview", Routes.AllThroughSchool(Urn).Overview),
            ("KS2", Routes.AllThroughSchool(Urn).KS2),
            ("KS4 headline measures", Routes.AllThroughSchool(Urn).KS4HeadlineMeasures),
            ("KS4 core subjects", Routes.AllThroughSchool(Urn).KS4CoreSubjects),
            ("Attendance", Routes.AllThroughSchool(Urn).Attendance),
            ("View similar schools", Routes.AllThroughSchool(Urn).ViewSimilarSchools),
            ("School details", Routes.AllThroughSchool(Urn).SchoolDetails),
            ("What is a similar school?", Routes.AllThroughSchool(Urn).WhatIsASimilarSchool),
            ("RISE resources", Routes.AllThroughSchool(Urn).RiseResources)
        ]);
        AssertSelectedNavigationItem(page, "Overview", Routes.AllThroughSchool(Urn).Overview);
    }

    [Fact]
    public async Task OverviewPage_WithoutSimilarSchools_OmitsViewSimilarSchoolsNavigation()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        AssertNavigation(page, [
            ("Overview", Routes.AllThroughSchool(Urn).Overview),
            ("KS2", Routes.AllThroughSchool(Urn).KS2),
            ("KS4 headline measures", Routes.AllThroughSchool(Urn).KS4HeadlineMeasures),
            ("KS4 core subjects", Routes.AllThroughSchool(Urn).KS4CoreSubjects),
            ("Attendance", Routes.AllThroughSchool(Urn).Attendance),
            ("School details", Routes.AllThroughSchool(Urn).SchoolDetails),
            ("What is a similar school?", Routes.AllThroughSchool(Urn).WhatIsASimilarSchool),
            ("RISE resources", Routes.AllThroughSchool(Urn).RiseResources)
        ]);
    }

    [Theory]
    [MemberData(nameof(AllThroughPages))]
    public async Task AllThroughPages_ShowHeadingAndSelectedNavigation(string path, string expectedHeading, string expectedNavigationText)
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(path);

        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be(expectedHeading);
        AssertSelectedNavigationItem(page, expectedNavigationText, path);
    }

    [Fact]
    public async Task OverviewPage_WhenAllThroughFeatureFlagDisabled_ReturnsNotFound()
    {
        SetupAllThroughSchool();
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableAllThroughSchools, false);

        await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OverviewPage_HasCollapsedMobileNavigationControl()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        var button = page.QuerySelector(".app-side-navigation__button");
        button.Should().NotBeNull();
        button!.GetAttribute("aria-expanded").Should().Be("false");
        button.GetAttribute("data-label-show").Should().Be("Show navigation");
        button.GetAttribute("data-label-hide").Should().Be("Hide navigation");
    }

    [Fact]
    public async Task AttendancePage_ShowsPrototypeContent()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance);

        page.QuerySelector(".govuk-caption-xl")!.TextContent.Trim().Should().Be("Test School 1");
        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be("Attendance measures");
        page.QuerySelector(".app-school-page p.govuk-body")!.TextContent.Trim().Should().Be("Compare this school's attendance measures with:");

        page.QuerySelectorAll(".app-school-page ul.govuk-list li")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "the local authority primary average",
                "the local authority secondary average",
                "the national primary average",
                "the national secondary average");

        page.QuerySelector(".govuk-inset-text")!.TextContent.Should().Contain("Monitor your school attendance service");
    }

    [Fact]
    public async Task AttendancePage_LinksToAllThroughSimilarSchoolsInfoAndVyedServices()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance);

        var links = page.QuerySelectorAll(".app-school-page a").ToArray();

        links[0].TextContent.Trim().Should().Be("how DfE identifies what a similar school is");
        links[0].GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);

        links[1].TextContent.Trim().Should().Be("View your education data (VYED) (opens in new tab)");
        links[1].GetAttribute("href").Should().Be("https://viewyourdata.education.gov.uk/");
        links[1].GetAttribute("target").Should().Be("_blank");
        links[1].GetAttribute("rel").Should().Be("noopener noreferrer");

        links[2].TextContent.Trim().Should().Be("get help on accessing VYED (opens in new tab)");
        links[2].GetAttribute("href").Should().Be("https://viewyourdata.education.gov.uk/Account/Help");
        links[2].GetAttribute("target").Should().Be("_blank");
        links[2].GetAttribute("rel").Should().Be("noopener noreferrer");
    }

    [Fact]
    public async Task SchoolDetailsPage_ShowsExpectedExternalLinks()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).SchoolDetails);

        var links = page.QuerySelectorAll(".govuk-summary-list a").ToArray();

        links.Should().Contain(x =>
            x.TextContent.Trim() == "View the latest Ofsted report (opens in new tab)"
            && x.GetAttribute("href") == $"https://reports.ofsted.gov.uk/provider/28/{Urn}"
            && x.GetAttribute("target") == "_blank"
            && x.GetAttribute("rel") == "noopener noreferrer");

        links.Should().Contain(x =>
            x.TextContent.Trim() == "Financial benchmarking and insights tool (opens in new tab)"
            && x.GetAttribute("href") == $"https://financial-benchmarking-and-insights-tool.education.gov.uk/school/{Urn}"
            && x.GetAttribute("target") == "_blank"
            && x.GetAttribute("rel") == "noopener noreferrer");

        links.Should().Contain(x =>
            x.TextContent.Trim() == "Get information about schools (opens in new tab)"
            && x.GetAttribute("href") == $"https://get-information-schools.service.gov.uk/Establishments/Establishment/Details/{Urn}"
            && x.GetAttribute("target") == "_blank"
            && x.GetAttribute("rel") == "noopener noreferrer");

        links.Should().Contain(x =>
            x.TextContent.Trim() == "View your education data (opens in new tab)"
            && x.GetAttribute("href") == "https://viewyourdata.education.gov.uk/"
            && x.GetAttribute("target") == "_blank"
            && x.GetAttribute("rel") == "noopener noreferrer");
    }

    [Fact]
    public async Task SchoolDetailsPage_HasCollapsedDetailsComponentsAndHomeBreadcrumb()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).SchoolDetails);

        page.QuerySelectorAll("details").Should().OnlyContain(x => x.GetAttribute("open") == null);

        var homeBreadcrumb = page.QuerySelector(".govuk-breadcrumbs__link");
        homeBreadcrumb.Should().NotBeNull();
        homeBreadcrumb!.TextContent.Trim().Should().Be("Home");
        homeBreadcrumb.GetAttribute("href").Should().Be(Routes.FindASchool());
    }

    public static TheoryData<string, string, string> AllThroughPages => new()
    {
        { Routes.AllThroughSchool(Urn).Overview, "Test School 1", "Overview" },
        { Routes.AllThroughSchool(Urn).KS2, "KS2 performance measures", "KS2" },
        { Routes.AllThroughSchool(Urn).KS4HeadlineMeasures, "KS4 headline performance measures", "KS4 headline measures" },
        { Routes.AllThroughSchool(Urn).KS4CoreSubjects, "KS4 core subject GCSE results", "KS4 core subjects" },
        { Routes.AllThroughSchool(Urn).Attendance, "Attendance measures", "Attendance" },
        { Routes.AllThroughSchool(Urn).ViewSimilarSchools, "View similar schools", "View similar schools" },
        { Routes.AllThroughSchool(Urn).SchoolDetails, "School details", "School details" },
        { Routes.AllThroughSchool(Urn).WhatIsASimilarSchool, "What is a similar school?", "What is a similar school?" },
        { Routes.AllThroughSchool(Urn).RiseResources, PageTitles.RiseResources, "RISE resources" }
    };

    private void SetupAllThroughSchool()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableAllThroughSchools, true);
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001").WithTypeOfEstablishment("28")),
            Build.Establishment("100002", "Test School 2", x => x.Open().AllThrough().InLA("001")));
    }

    private static void AssertNavigation(IDocument page, (string Text, string Href)[] expectedItems)
    {
        var navigationItems = page.QuerySelectorAll(".app-side-navigation__item a");

        navigationItems.Select(x => x.TextContent.Trim()).Should().Equal(expectedItems.Select(x => x.Text));
        navigationItems.Select(x => x.GetAttribute("href")).Should().Equal(expectedItems.Select(x => x.Href));
    }

    private static void AssertSelectedNavigationItem(IDocument page, string expectedText, string expectedHref)
    {
        var selectedItem = page.QuerySelector(".app-side-navigation__item--selected");
        selectedItem.Should().NotBeNull();
        selectedItem!.TextContent.Trim().Should().Be(expectedText);

        var link = selectedItem.QuerySelector("a");
        link.Should().NotBeNull();
        link!.GetAttribute("href").Should().Be(expectedHref);
        link.ClassList.Should().Contain("app-side-navigation__link--selected");
        link.GetAttribute("aria-current").Should().Be("page");
    }
}
