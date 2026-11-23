using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Text;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

/// <summary>
/// KS2 progress scores (KS2-PROG-02 school page, KS2-PROG-06 comparison page), with EnableKs2ProgressMeasures on.
/// </summary>
public class Ks2ProgressScoresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string NoProgressScoresMessage = "There are no KS1-KS2 progress scores";

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableKs2ProgressMeasures);
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableKs2ProgressNumberOfPupils);
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);
        return base.DisposeAsync();
    }

    private void SetupPrimarySchoolWithSimilarSchools(bool allThrough = false)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => (allThrough ? x.Open().AllThrough() : x.Open().Primary()).InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002", "100003"]))
            .SetupValues(Build.PrimaryValues(["100001", "100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithProgress("Reading", current: "z", prev: "z", prev2: "-1.84")
                .WithProgress("Writing", current: "z", prev: "z", prev2: "2.06")
                .WithEligiblePupils(current: "30", prev: "28", prev2: "58")
                .WithProgressIncluded("Reading", current: "", prev: "", prev2: "95%")
                .WithProgressIncluded("Writing", current: "", prev: "", prev2: "88%")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithProgress("Reading", current: "z", prev: "z", prev2: "3.0")
                .WithEligiblePupils(current: "45", prev: "44", prev2: "43")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithProgress("Reading", current: "z", prev: "z", prev2: "-2.0")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithProgress("Reading", current: "z", prev: "z", prev2: "-0.46")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithProgress("Reading", current: "z", prev: "z", prev2: "0.04")));
    }

    // ---------- School page ----------

    [Fact]
    public async Task SchoolPage_ProgressScoresAreTheFirstMeasure_AndReplaceTheMessage()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        var firstHeading = page.QuerySelector(".app-measure-section h2");
        firstHeading!.TrimmedTextContent().Should().Be("Progress scores in reading, writing and maths");
        page.ElementWithTestIdShouldExist("progress-rwm-heading");
        page.QuerySelector(".app-measure-message-panel").Should().BeNull();
        page.Source.Text.Should().NotContain(NoProgressScoresMessage);
    }

    [Fact]
    public async Task SchoolPage_DetailsComponent_IsCollapsedAndLinksOpenInANewTab()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        var details = page.ElementWithTestIdShouldExist("progress-rwm-details");
        details.TagName.Should().BeEquivalentTo("details");
        details.HasAttribute("open").Should().BeFalse();

        var technicalGuidance = page.ElementWithTestIdShouldExist<IHtmlAnchorElement>("progress-rwm-technical-guidance-link");
        technicalGuidance.Href.Should().Be("https://www.gov.uk/government/publications/primary-school-accountability");
        technicalGuidance.Target.Should().Be("_blank");
        technicalGuidance.TrimmedTextContent().Should().StartWith("Technical guidance");

        var ees = page.ElementWithTestIdShouldExist<IHtmlAnchorElement>("progress-rwm-ees-link");
        ees.Href.Should().Be("https://explore-education-statistics.service.gov.uk/");
        ees.Target.Should().Be("_blank");
    }

    [Fact]
    public async Task SchoolPage_SubjectFilter_DefaultsToReadingWithOptionsInOrder()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        var select = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("progress-subject-filter");
        select.Options.Select(o => o.TrimmedTextContent()).Should().Equal("Reading", "Writing", "Maths");
        select.Options.Single(o => o.IsSelected).TrimmedTextContent().Should().Be("Reading");
        page.QuerySelector("label[for='progress-subject-select']")!.TrimmedTextContent().Should().Be("Subject");
    }

    [Fact]
    public async Task SchoolPage_Tabs_ChartsTableAndTopPerformers()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        page.ElementWithTestIdShouldExist("progress-tabs").ChildTrimmedTextContent()
            .Should().Equal("Charts", "Table", "Top performers");
    }

    [Fact]
    public async Task SchoolPage_NoSimilarSchools_HasNoTopPerformersTab()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        page.ElementWithTestIdShouldExist("progress-tabs").ChildTrimmedTextContent()
            .Should().Equal("Charts", "Table");
    }

    [Fact]
    public async Task SchoolPage_Table_ShowsScoresTo1DecimalPlace()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-table-view-table").ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "-1.8", "No available data", "No available data"],
            ["Similar schools average", "0.5", "No available data", "No available data"],
            ["Local authority schools average", "-0.5", "No available data", "No available data"],
            ["Schools in England average", "0.0", "No available data", "No available data"]);
    }

    [Fact]
    public async Task SchoolPage_WritingFilter_UpdatesTheProgressScoresOnly()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync($"{Routes.PrimarySchool("100001").KS2}?progress-subject=w");

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-table-view-table");
        table.Rows[1].Cells.Select(c => c.TrimmedTextContent()).Skip(1).First().Should().Be("2.1");

        // Other KS2 measures keep their own default filters.
        page.ElementWithTestIdShouldExist<IHtmlSelectElement>("expected-rwm-subject-filter")
            .Options.Single(o => o.IsSelected).TrimmedTextContent().Should().Be("Reading, writing and maths");
    }

    [Fact]
    public async Task SchoolPage_NumberOfPupils_UnderEachViewForTheSchoolAndTopPerformers()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        foreach (var id in new[] { "progress-current-year-number-of-pupils", "progress-year-by-year-number-of-pupils", "progress-table-number-of-pupils", "progress-top-performers-number-of-pupils" })
        {
            var details = page.ElementWithTestIdShouldExist(id);
            details.HasAttribute("open").Should().BeFalse();
        }

        page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-table-number-of-pupils-table").ShouldHaveRows(
            ["School(s)", "Pupils", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "Eligible pupils at school", "58", "28", "30"],
            ["Proportion of pupils included in the measure", "95%", "No available data", "No available data"]);

        // Top performers are listed in rank order (latest year has no scores, so only those with data).
        page.ElementWithTestIdShouldExist("progress-top-performers-number-of-pupils")
            .QuerySelectorAll("tbody th[scope='rowgroup']").Select(th => th.TrimmedTextContent())
            .Should().Equal(page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-top-performers-table")
                .QuerySelectorAll("tbody th").Select(th => th.TrimmedTextContent()));
    }

    [Fact]
    public async Task SchoolPage_NumberOfPupils_ProportionFollowsTheSubjectFilter()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync($"{Routes.PrimarySchool("100001").KS2}?progress-subject=w");

        page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-table-number-of-pupils-table").ShouldHaveRows(
            ["School(s)", "Pupils", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "Eligible pupils at school", "58", "28", "30"],
            ["Proportion of pupils included in the measure", "88%", "No available data", "No available data"]);
    }

    [Fact]
    public async Task SchoolPage_NumberOfPupilsFlagOff_HasNoNumberOfPupils()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableKs2ProgressNumberOfPupils, false);
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        page.ElementWithTestIdShouldExist("progress-rwm-heading");
        page.QuerySelector("details.app-number-of-pupils").Should().BeNull();
    }

    [Fact]
    public async Task SchoolPage_ProgressFlagOff_ShowsTheMessageAndNoProgressMeasure()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableKs2ProgressMeasures, false);
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        page.QuerySelector(".app-measure-message-panel")!.TrimmedTextContent().Should().Contain(NoProgressScoresMessage);
        page.QuerySelector("[data-testid='progress-tabs']").Should().BeNull();
        page.QuerySelector("details.app-number-of-pupils").Should().BeNull();
    }

    [Fact]
    public async Task AllThroughSchoolPage_ShowsProgressScores()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);
        SetupPrimarySchoolWithSimilarSchools(allThrough: true);

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool("100001").KS2);

        page.ElementWithTestIdShouldExist("progress-rwm-heading").TrimmedTextContent()
            .Should().Be("Progress scores in reading, writing and maths");
        page.ElementWithTestIdShouldExist("progress-tabs").ChildTrimmedTextContent()
            .Should().Equal("Charts", "Table", "Top performers");
        page.Source.Text.Should().NotContain(NoProgressScoresMessage);
    }

    // ---------- Comparing to page ----------

    [Fact]
    public async Task ComparisonPage_ShowsProgressScoresForBothSchoolsAndEngland()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").Ks2);

        page.QuerySelector(".app-measure-section h2")!.TrimmedTextContent()
            .Should().Be("Progress scores in reading, writing and maths");
        page.Source.Text.Should().NotContain(NoProgressScoresMessage);
        page.ElementWithTestIdShouldExist("progress-tabs").ChildTrimmedTextContent()
            .Should().Equal("Charts", "Table");

        page.ElementWithTestIdShouldExist<IHtmlTableElement>("progress-table-view-table").ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "-1.8", "No available data", "No available data"],
            ["Test School 2", "3.0", "No available data", "No available data"],
            ["Schools in England average", "0.0", "No available data", "No available data"]);
    }

    [Fact]
    public async Task ComparisonPage_NumberOfPupils_ForBothSchools()
    {
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").Ks2);

        page.QuerySelector("[data-testid='progress-top-performers-number-of-pupils']").Should().BeNull();
        page.ElementWithTestIdShouldExist("progress-current-year-number-of-pupils")
            .QuerySelectorAll("tbody th[scope='rowgroup']").Select(th => th.TrimmedTextContent())
            .Should().Equal("Test School 1", "Test School 2");
    }

    [Fact]
    public async Task ComparisonPage_ProgressFlagOff_ShowsTheMessage()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableKs2ProgressMeasures, false);
        SetupPrimarySchoolWithSimilarSchools();

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").Ks2);

        page.QuerySelector(".app-measure-message-panel")!.TrimmedTextContent().Should().Contain(NoProgressScoresMessage);
        page.QuerySelector("[data-testid='progress-tabs']").Should().BeNull();
    }

    [Fact]
    public async Task AllThroughComparisonPage_ShowsProgressScores()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);
        SetupPrimarySchoolWithSimilarSchools(allThrough: true);

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool("100001").PrimaryComparison("100002").Ks2);

        page.ElementWithTestIdShouldExist("progress-rwm-heading");
        page.ElementWithTestIdShouldExist("progress-tabs").ChildTrimmedTextContent()
            .Should().Equal("Charts", "Table");
    }
}
