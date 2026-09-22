using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
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
            ("Attendance", Routes.AllThroughSchool(Urn).Attendance),
            ("View similar schools", Routes.AllThroughSchool(Urn).ViewSimilarSchools),
            ("School details", Routes.AllThroughSchool(Urn).SchoolDetails),
            ("What is a similar school?", Routes.AllThroughSchool(Urn).WhatIsASimilarSchool),
            ("RISE resources", Routes.AllThroughSchool(Urn).RiseResources)
        ]);
        AssertSelectedNavigationItem(page, "Overview", Routes.AllThroughSchool(Urn).Overview);
    }

    [Fact]
    public async Task OverviewPage_WithSecondarySimilarSchools_ShowsSecondaryPhasePrototypeContent()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be("Test School 1");
        page.QuerySelector(".app-overview__address")!.TextContent.Trim().Should().Be("1 Test Street, Test Town, TT1 1TT");
        page.QuerySelector(".app-overview__lead")!.TextContent.Trim()
            .Should().Be("We've identified 50 similar secondary phase schools (including all-throughs) to help you:");

        page.QuerySelectorAll(".app-overview__list").First().QuerySelectorAll("li")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "compare performance data",
                "find improvement opportunities",
                "connect with school leaders and share insights");

        var link = page.QuerySelector(".app-overview a");
        link.Should().NotBeNull();
        link!.TextContent.Trim().Should().Be("how the DfE defines what a similar school is");
        link.GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);

        page.QuerySelectorAll(".app-overview h2")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "Compare school performance",
                "View similar schools to connect with",
                "Missing data for multi-phase schools");
    }

    [Fact]
    public async Task OverviewPage_WithSecondarySimilarSchools_ShowsSecondaryPhaseNavigation()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        AssertNavigation(page, [
            ("Overview", Routes.AllThroughSchool(Urn).Overview),
            ("KS4 headline measures", Routes.AllThroughSchool(Urn).KS4HeadlineMeasures),
            ("KS4 core subjects", Routes.AllThroughSchool(Urn).KS4CoreSubjects),
            ("Attendance", Routes.AllThroughSchool(Urn).Attendance),
            ("View similar schools", Routes.AllThroughSchool(Urn).ViewSimilarSchools),
            ("School details", Routes.AllThroughSchool(Urn).SchoolDetails),
            ("What is a similar school?", Routes.AllThroughSchool(Urn).WhatIsASimilarSchool),
            ("RISE resources", Routes.AllThroughSchool(Urn).RiseResources)
        ]);
    }

    [Fact]
    public async Task OverviewPage_WithPrimaryAndSecondarySimilarSchools_ShowsBothPhasePrototypeContent()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        page.QuerySelector(".app-overview__lead")!.TextContent.Trim()
            .Should().Be("For all-through schools or cross-phase middle schools, we identify:");

        page.QuerySelectorAll(".app-overview__list").First().QuerySelectorAll("li")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "50 primary schools, including all-throughs, similar to the school\u2019s primary phase",
                "50 secondary schools, including all-throughs, similar to the school\u2019s secondary phase");
    }

    [Fact]
    public async Task OverviewPage_WithoutSimilarSchools_ShowsEmptyStateContent()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be("Test School 1");
        page.QuerySelector(".app-overview__address")!.TextContent.Trim().Should().Be("1 Test Street, Test Town, TT1 1TT");
        page.QuerySelector(".app-overview h2.govuk-heading-m")!.TextContent.Trim().Should().Be("There are no similar schools available for this school");
        page.QuerySelector(".app-overview__lead").Should().BeNull();
        page.QuerySelectorAll(".app-overview a").Should().BeEmpty();
    }

    [Fact]
    public async Task OverviewPage_WithoutSimilarSchools_OmitsViewSimilarSchoolsNavigation()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

        AssertNavigation(page, [
            ("Overview", Routes.AllThroughSchool(Urn).Overview),
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
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));
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
    public async Task ViewSimilarSchoolsPage_ShowsAllThroughStaticContentAndPrimaryTabByDefault()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).ViewSimilarSchools);

        page.QuerySelector(".govuk-caption-xl")!.TextContent.Trim().Should().Be("Test School 1");
        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be("View similar schools");

        NormaliseWhitespace(page.QuerySelector(".app-all-through-similar-schools p.govuk-body")!.TextContent)
            .Should().Be("We've identified 50 similar primary phase schools (including all-throughs) and 50 similar secondary phase schools (including all-throughs). Select primary or secondary then choose a school to compare further and connect with for support. You can also refine the results by applying filters.");

        var link = page.QuerySelector(".app-all-through-similar-schools p.govuk-body a");
        link.Should().NotBeNull();
        link!.TextContent.Trim().Should().Be("how DfE identifies what a similar school is");
        link.GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);
        link.GetAttribute("target").Should().BeNull();

        var tabs = page.QuerySelectorAll(".app-phase-tabs__tab").ToArray();
        tabs.Select(x => x.TextContent.Trim()).Should().Equal("Primary", "Secondary");
        tabs.Select(x => x.GetAttribute("role")).Should().Equal("tab", "tab");

        tabs[0].GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).ViewSimilarSchools);
        tabs[0].GetAttribute("aria-selected").Should().Be("true");
        tabs[0].ParentElement!.ClassList.Should().Contain("app-phase-tabs__list-item--selected");

        tabs[1].GetAttribute("href").Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary");
        tabs[1].GetAttribute("aria-selected").Should().Be("false");
    }

    [Fact]
    public async Task ViewSimilarSchoolsPage_WithSecondaryPhase_ShowsSecondaryTabAsActive()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary");

        var tabs = page.QuerySelectorAll(".app-phase-tabs__tab").ToArray();
        tabs[0].GetAttribute("aria-selected").Should().Be("false");
        tabs[1].GetAttribute("aria-selected").Should().Be("true");
        tabs[1].ParentElement!.ClassList.Should().Contain("app-phase-tabs__list-item--selected");
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
    public async Task Ks4CoreSubjectsPage_ShowsExpectedStaticContentAndMeasures()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS4CoreSubjects);

        page.QuerySelector(".govuk-caption-xl")!.TextContent.Trim().Should().Be("Test School 1");
        page.QuerySelector("h1.govuk-heading-xl")!.TextContent.Trim().Should().Be("KS4 core subject GCSE results");
        page.QuerySelector(".app-school-page p.govuk-body")!.TextContent.Trim().Should().Be("Compare this school's GCSEs in KS4 core subjects with:");

        page.QuerySelectorAll(".app-school-page ul.govuk-list li")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "50 similar secondary phase schools (including all-throughs)",
                "the local authority average",
                "the national average");

        var similarSchoolInfoLink = page.QuerySelector(".app-school-page p.govuk-body a");
        similarSchoolInfoLink.Should().NotBeNull();
        similarSchoolInfoLink!.TextContent.Trim().Should().Be("how DfE identifies what a similar school is");
        similarSchoolInfoLink.GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);

        page.QuerySelectorAll(".app-measure-section h2")
            .Select(x => x.TextContent.Trim())
            .Should().Equal(
                "English language",
                "English literature",
                "Maths",
                "Combined science (double award)",
                "Biology",
                "Chemistry",
                "Physics");
    }

    [Fact]
    public async Task Ks4CoreSubjectsPage_WithSecondarySimilarSchools_ShowsTopPerformersTabs()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(Build.SecondaryGroup(Urn, ["100002"]));
        Fixture.SimilarSchoolsSecondaryRepository.SetupValues(Build.SecondaryValues([Urn, "100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS4CoreSubjects);

        page.ElementWithTestIdShouldExist("eng-lang-tabs")
            .ChildTrimmedTextContent()
            .Should().BeEquivalentTo("Charts", "Table", "Top performers");

        page.ElementWithTestIdShouldExist("comb-sci-tabs")
            .ChildTrimmedTextContent()
            .Should().BeEquivalentTo("Charts", "Table", "Top performers");
    }

    [Fact]
    public async Task Ks4CoreSubjectsPage_WithoutSecondarySimilarSchools_HidesTopPerformersTabs()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS4CoreSubjects);

        page.ElementWithTestIdShouldExist("eng-lang-tabs")
            .ChildTrimmedTextContent()
            .Should().BeEquivalentTo("Charts", "Table");

        page.ElementWithTestIdShouldExist("comb-sci-tabs")
            .ChildTrimmedTextContent()
            .Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Ks4CoreSubjectsPage_GradeFilters_HaveExpectedOptions()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS4CoreSubjects);

        page.ElementWithTestIdShouldExist("eng-lang-grade-filter")
            .ChildTrimmedTextContent()
            .Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);

        page.ElementWithTestIdShouldExist("comb-sci-grade-filter")
            .ChildTrimmedTextContent()
            .Should().Equal(["Grade 4-4 and above", "Grade 5-5 and above", "Grade 7-7 and above"]);
    }

    [Fact]
    public async Task Ks4CoreSubjectsPage_TopPerformers_LinkToSecondaryComparisonAndAllThroughSecondaryTab()
    {
        SetupAllThroughSchool();
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001").WithTypeOfEstablishment("28").WithAddress("1 Test Street", "", "", "Test Town", "TT1 1TT")),
            Build.Establishment("100002", "Test School 2", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100005", "Test School 5", x => x.Open().AllThrough().InLA("001")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup(Urn, ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment(Urn, x => x.WithEngLang49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLang49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngLang49(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS4CoreSubjects);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("eng-lang-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary");

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lang-top-performers-table");
        var topPerformersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerformersLinks.Should().BeEquivalentTo([
            Routes.AllThroughSchool(Urn).SecondaryComparison("100004").Similarity,
            Routes.AllThroughSchool(Urn).SecondaryComparison("100003").Similarity,
            Routes.AllThroughSchool(Urn).SecondaryComparison("100002").Similarity
        ]);
    }

    [Fact]
    public async Task Ks2PerformanceMeasuresPage_TopPerformers_LinkToPrimaryComparison()
    {
        SetupAllThroughSchool();
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001").WithTypeOfEstablishment("28").WithAddress("1 Test Street", "", "", "Test Town", "TT1 1TT")),
            Build.Establishment("100002", "Test School 2", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Open().AllThrough().InLA("001")),
            Build.Establishment("100005", "Test School 5", x => x.Open().AllThrough().InLA("001")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup(Urn, ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(Urn, x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).KS2);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("expected-rwm-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-top-performers-table");
        var topPerformersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerformersLinks.Should().BeEquivalentTo([
            Routes.AllThroughSchool(Urn).PrimaryComparison("100004").Similarity,
            Routes.AllThroughSchool(Urn).PrimaryComparison("100003").Similarity,
            Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Similarity
        ]);
    }

    [Fact]
    public async Task PrimaryComparisonPage_AllThroughRoute_PreservesAllThroughNavigation()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup(Urn, ["100002"]))
            .SetupValues(Build.PrimaryValues([Urn, "100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Similarity);

        page.ElementWithTestIdShouldExist<IHtmlAnchorElement>("what-is-a-similar-school-link")
            .PathName.Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);

        page.QuerySelector(".govuk-back-link")!
            .GetAttribute("href").Should().Be(Routes.AllThroughSchool(Urn).ViewSimilarSchools);

        page.QuerySelectorAll(".compare-nav a")
            .Select(l => l.GetAttribute("href"))
            .Should().Equal(
                Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Similarity,
                Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Ks2,
                Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Attendance,
                Routes.AllThroughSchool(Urn).PrimaryComparison("100002").SchoolDetails);
    }

    [Fact]
    public async Task SecondaryComparisonPage_AllThroughRoute_PreservesAllThroughNavigation()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository
            .SetupGroups(Build.SecondaryGroup(Urn, ["100002"]))
            .SetupValues(Build.SecondaryValues([Urn, "100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.AllThroughSchool(Urn).SecondaryComparison("100002").Similarity);

        page.ElementWithTestIdShouldExist<IHtmlAnchorElement>("what-is-a-similar-school-link")
            .PathName.Should().Be(Routes.AllThroughSchool(Urn).WhatIsASimilarSchool);

        page.QuerySelector(".govuk-back-link")!
            .GetAttribute("href").Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary");

        page.QuerySelectorAll(".compare-nav a")
            .Select(l => l.GetAttribute("href"))
            .Should().Equal(
                Routes.AllThroughSchool(Urn).SecondaryComparison("100002").Similarity,
                Routes.AllThroughSchool(Urn).SecondaryComparison("100002").KS4HeadlineMeasures,
                Routes.AllThroughSchool(Urn).SecondaryComparison("100002").KS4CoreSubjects,
                Routes.AllThroughSchool(Urn).SecondaryComparison("100002").Attendance,
                Routes.AllThroughSchool(Urn).SecondaryComparison("100002").SchoolDetails);
    }

    [Fact]
    public async Task PrimaryComparisonAttendancePage_AllThroughRoute_UsesPrimaryEnglandAverageLabel()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup(Urn, ["100002"]))
            .SetupValues(Build.PrimaryValues([Urn, "100002"]));
        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment(Urn, x => x.WithOverallAbsence(current: "6.91", previous: "6.90", previous2: "6.89")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "5.12", previous: "5.11", previous2: "5.10")));
        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "4.83", previous: "4.82", previous2: "4.81")));

        var page = await Fixture.RequestPageAsync(
            Routes.AllThroughSchool(Urn).PrimaryComparison("100002").Attendance);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.89%", "6.90%", "6.91%"],
            ["Test School 2", "5.10%", "5.11%", "5.12%"],
            ["Primary schools in England average", "4.81%", "4.82%", "4.83%"]);
    }

    [Fact]
    public async Task SecondaryComparisonAttendancePage_AllThroughRoute_UsesSecondaryEnglandAverageLabel()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsSecondaryRepository
            .SetupGroups(Build.SecondaryGroup(Urn, ["100002"]))
            .SetupValues(Build.SecondaryValues([Urn, "100002"]));
        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment(Urn, x => x.WithOverallAbsence(current: "6.91", previous: "6.90", previous2: "6.89")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "5.12", previous: "5.11", previous2: "5.10")));
        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "4.83", previous: "4.82", previous2: "4.81")));

        var page = await Fixture.RequestPageAsync(
            Routes.AllThroughSchool(Urn).SecondaryComparison("100002").Attendance);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.89%", "6.90%", "6.91%"],
            ["Test School 2", "5.10%", "5.11%", "5.12%"],
            ["Secondary schools in England average", "4.81%", "4.82%", "4.83%"]);
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
            x.TextContent.Trim() == "View your education data (VYED) (opens in new tab)"
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

    [Fact]
    public async Task OverviewPage_HomeBreadcrumb_LinksToFindASchool()
    {
        SetupAllThroughSchool();
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(Build.PrimaryGroup(Urn, ["100002"]));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Overview);

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
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001").WithTypeOfEstablishment("28").WithAddress("1 Test Street", "", "", "Test Town", "TT1 1TT")),
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

    private static string NormaliseWhitespace(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
