using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class AllPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string PrimarySchoolUrn = "100001";
    private static readonly string ComparisonPath = Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparison("100002");

    private static readonly PageTestCase[] PrimaryPages = [
        new(Routes.PrimarySchool(PrimarySchoolUrn).Overview, "Test School 1", NavigationText: "Overview", IsOverviewPage: true),
        new(Routes.PrimarySchool(PrimarySchoolUrn).KS2, "KS2 performance measures", NavigationText: "KS2"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).Attendance, "Attendance"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).ViewSimilarSchools, "View similar schools"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).SchoolDetails, "School details"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool, "What is a similar school?"),
        new(ComparisonPath, "Test School 2", IsInNavigation: false)
    ];

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

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

        if (path == ComparisonPath)
        {
            navigationItems.Should().SatisfyRespectively(
                n => n.ShouldLinkTo("Home", Routes.FindASchool()),
                n => n.ShouldLinkTo("View similar schools", Routes.PrimarySchool(PrimarySchoolUrn).ViewSimilarSchools));
        }
        else
        {
            navigationItems.Should().SatisfyRespectively(n => n.ShouldLinkTo("Home", Routes.FindASchool()));
        }
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetails_ContainsExpectedSections()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonSchoolDetails("100002"));

        var headings = page.QuerySelectorAll("h2").Select(h => h.TrimmedTextContent()).ToList();

        headings.Should().Contain("School Details");
        headings.Should().Contain("Location");
        headings.Should().Contain("Further information");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetails_HeadingAndTitle_ReflectComparisonPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonSchoolDetails("100002"));

        page.Title.Should().Be("School details compared to Test School 2 - Get school improvement insights - GOV.UK");

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading.TrimmedTextContent().Should().Be("Test School 2");

        var caption = page.QuerySelector(".govuk-caption-xl");
        caption.Should().NotBeNull();
        caption.TrimmedTextContent().Should().Be("Test School 1");
    }

    [Fact]
    public async Task SchoolDetails_OfstedReportLink_UsesPrimaryProviderType()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(PrimarySchoolUrn).SchoolDetails);

        var ofstedLink = page.QuerySelector("a[href*='reports.ofsted.gov.uk']");
        ofstedLink.Should().NotBeNull();
        ofstedLink!.GetAttribute("href").Should().Be($"https://reports.ofsted.gov.uk/provider/21/{PrimarySchoolUrn}");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetails_OfstedReportLink_UsesPrimaryProviderType()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonSchoolDetails("100002"));

        var ofstedLink = page.QuerySelector("a[href*='reports.ofsted.gov.uk']");
        ofstedLink.Should().NotBeNull();
        ofstedLink!.GetAttribute("href").Should().Be("https://reports.ofsted.gov.uk/provider/21/100002");
    }

    [Theory]
    [MemberData(nameof(AllPagesWithSideNavigation))]
    public async Task AllPages_Navigation_ShowsLinksInCorrectOrder(string path)
    {
        var page = await Fixture.RequestPageAsync(path);

        var navigationItems = page.QuerySelectorAll(".app-side-navigation__item a");

        var navigationAssertions = PrimaryPages
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
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(PrimarySchoolUrn).Overview);

        var link = page.QuerySelector(".app-body-container-with-side-navigation a");
        link.Should().NotBeNull();
        link.GetAttribute("href").Should().Be(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool);
    }

    [Fact]
    public async Task WhatIsASimilarSchoolPage_ContainsViewSimilarSchoolsLink()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool);

        var links = page.QuerySelectorAll(".app-body-container-with-side-navigation a");
        links.Should().Contain(l => l.GetAttribute("href") == Routes.PrimarySchool(PrimarySchoolUrn).ViewSimilarSchools);
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

    public static TheoryData<string> AllPagesWithSideNavigation()
    {
        var data = new TheoryData<string>();
        foreach (var page in PrimaryPages)
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
        foreach (var page in PrimaryPages)
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
        foreach (var page in PrimaryPages)
        {
            data.Add(page.Path, page.Heading, page.IsOverviewPage);
        }

        return data;
    }

    private record PageTestCase(string Path, string Heading, string? NavigationText = null, bool IsOverviewPage = false, bool IsInNavigation = true);
}
