using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Core.Services.Helper;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonKs2PerformanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string PrimarySchoolUrn = "100001";
    private const string SimilarSchoolUrn = "100002";

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(PrimarySchoolUrn, "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment(SimilarSchoolUrn, "Test School 2", x => x.Open().Primary().InLA("002")));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task SimilarSchoolComparison_Ks2_DisplaysProgressScoreSection()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabHeading = page.QuerySelector("h2.govuk-heading-l");
        tabHeading.Should().NotBeNull();
        tabHeading.TrimmedTextContent().Should().Be("KS2 performance measures");

        var progressHeading = page.QuerySelector("#progress-rwm-heading");
        progressHeading.Should().NotBeNull();
        progressHeading.TrimmedTextContent().Should().Be("Progress score in reading, writing and maths");

        var insetPanel = page.QuerySelector(".app-measure-message-panel");
        insetPanel.Should().NotBeNull();
        insetPanel.TrimmedTextContent().Should().Contain("There are no KS1-KS2 progress scores");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("expected-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Meeting expected standard in reading, writing and maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about meeting the expected standard");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("expected-rwm-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithRwmExpected(current: "60", prev: "61", prev2: "62")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "61", prev: "60", prev2: "59")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "62%", "61%", "60%"],
            ["Schools in England average", "59%", "60%", "61%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithRwmExpected(current: "81.0", prev: "80.3", prev2: "78.5")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithRwmExpected(current: "59.8", prev: "61.4", prev2: "62.2")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "60.5", prev: "60.4", prev2: "59.3")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "62%", "61%", "60%"],
            ["Schools in England average", "59%", "60%", "61%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("expected-rwm-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("expected-rwm-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("expected-rwm-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("expected-rwm-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var filter = page.ElementWithTestIdShouldExist("expected-rwm-subject-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Reading, writing and maths", "Reading", "Writing", "Maths"]);
    }

    [InlineData("Reading", new[] { "70%", "71%", "72%" }, new[] { "50%", "51%", "52%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Writing", new[] { "60%", "61%", "62%" }, new[] { "40%", "41%", "42%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Maths", new[] { "50%", "51%", "52%" }, new[] { "30%", "31%", "32%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task MeetingExpectedStandardRwm_SubjectFilter_UpdatesTableViewWithSubjectValues(
        string filterOption, string[] currentSchool, string[] similarSchool, string[] england)
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x
                .WithRwmExpectedReading(current: "52", prev: "51", prev2: "50")
                .WithRwmExpectedWriting(current: "42", prev: "41", prev2: "40")
                .WithRwmExpectedMaths(current: "32", prev: "31", prev2: "30")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmExpectedReading(current: "74", prev: "73", prev2: "72")
                .WithRwmExpectedWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmExpectedMaths(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("expected-rwm-subject-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("expected-rwm-subject-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchool],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("higher-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Achieved a higher standard in reading, writing and maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about achieving the higher standard");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("higher-rwm-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithRwmHigher(current: "31", prev: "30", prev2: "29")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithRwmHigher(current: "20", prev: "21", prev2: "22")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "21", prev: "20", prev2: "19")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "29%", "30%", "31%"],
            ["Test School 2", "22%", "21%", "20%"],
            ["Schools in England average", "19%", "20%", "21%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithRwmHigher(current: "31.0", prev: "30.3", prev2: "28.5")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithRwmHigher(current: "19.8", prev: "21.4", prev2: "22.2")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "20.5", prev: "20.4", prev2: "19.3")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "29%", "30%", "31%"],
            ["Test School 2", "22%", "21%", "20%"],
            ["Schools in England average", "19%", "20%", "21%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("higher-rwm-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("higher-rwm-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task AchievedExpectedStandardRwm_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("higher-rwm-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("higher-rwm-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var filter = page.ElementWithTestIdShouldExist("higher-rwm-subject-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Reading, writing and maths", "Reading", "Writing", "Maths"]);
    }

    [InlineData("Reading", new[] { "20%", "21%", "22%" }, new[] { "10%", "11%", "12%" }, new[] { "22%", "23%", "24%" })]
    [InlineData("Writing", new[] { "15%", "16%", "17%" }, new[] { "5%", "6%", "7%" }, new[] { "17%", "18%", "19%" })]
    [InlineData("Maths", new[] { "10%", "11%", "12%" }, new[] { "0%", "1%", "2%" }, new[] { "12%", "13%", "14%" })]
    [Theory]
    public async Task AchievedHigherStandardRwm_SubjectFilter_UpdatesTableViewWithSubjectValues(
        string filterOption, string[] currentSchool, string[] similarSchool, string[] england)
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x
                .WithRwmHigherReading(current: "22", prev: "21", prev2: "20")
                .WithRwmHigherWriting(current: "17", prev: "16", prev2: "15")
                .WithRwmHigherMaths(current: "12", prev: "11", prev2: "10")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x
                .WithRwmHigherReading(current: "12", prev: "11", prev2: "10")
                .WithRwmHigherWriting(current: "7", prev: "6", prev2: "5")
                .WithRwmHigherMaths(current: "2", prev: "1", prev2: "0")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmHigherReading(current: "24", prev: "23", prev2: "22")
                .WithRwmHigherWriting(current: "19", prev: "18", prev2: "17")
                .WithRwmHigherMaths(current: "14", prev: "13", prev2: "12")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("higher-rwm-subject-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("higher-rwm-subject-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchool],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("reading-score-heading");
        heading.TrimmedTextContent().Should().Be("Average scaled score in reading");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about average scaled score in reading");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task AverageScaledScoreReading_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("higher-rwm-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task AverageScaledScoreReading_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithReadingScaledScore(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithReadingScaledScore(current: "103.2", prev: "102.2", prev2: "101.2")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.4", prev: "106.6", prev2: "105.8")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Test School 2", "101.2", "102.2", "103.2"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TableView_ValuesRoundTo1DecimalPlace()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithReadingScaledScore(current: "101.41", prev: "100.43", prev2: "99.42")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithReadingScaledScore(current: "103.24", prev: "102.20", prev2: "101.15")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.42", prev: "106.59", prev2: "105.82")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Test School 2", "101.2", "102.2", "103.2"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("reading-score-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "80"),
            ("axis-step", "20"),
            ("axis-max", "120"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("reading-score-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "80"),
            ("axis-step", "20"),
            ("axis-max", "120"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));
    }

    [Fact]
    public async Task AverageScaledScoreReading_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("reading-score-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("reading-score-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task AverageScaledScoreMaths_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("maths-score-heading");
        heading.TrimmedTextContent().Should().Be("Average scaled score in maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about average scaled score in maths");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task AverageScaledScoreMaths_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("reading-score-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithMathsScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithMathsScaledScore(current: "104.2", prev: "103.2", prev2: "102.2")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "100.4", "101.4", "102.4"],
            ["Test School 2", "102.2", "103.2", "104.2"],
            ["Schools in England average", "106.8", "107.6", "108.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TableView_ValuesRoundTo1DecimalPlace()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithMathsScaledScore(current: "102.42", prev: "101.41", prev2: "100.39")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithMathsScaledScore(current: "104.21", prev: "103.22", prev2: "102.19")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "108.37", prev: "107.61", prev2: "106.79")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "100.4", "101.4", "102.4"],
            ["Test School 2", "102.2", "103.2", "104.2"],
            ["Schools in England average", "106.8", "107.6", "108.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("maths-score-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "80"),
            ("axis-step", "20"),
            ("axis-max", "120"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("maths-score-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "80"),
            ("axis-step", "20"),
            ("axis-max", "120"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("maths-score-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("maths-score-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("expected-gps-heading");
        heading.TrimmedTextContent().Should().Be("Meeting expected standard in grammar, punctuation and spelling");

        var section = heading.Closest(".app-measure-section");
        section.Should().NotBeNull();

        var details = section!.QuerySelector("details.govuk-details");
        details.Should().NotBeNull();
        details!.QuerySelector(".govuk-details__summary-text")!.TrimmedTextContent()
            .Should().Be("Information about meeting the expected standard");
        details.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("maths-score-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "69", prev: "68", prev2: "67")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "60%", "61%", "62%"],
            ["Test School 2", "75%", "76%", "77%"],
            ["Schools in England average", "67%", "68%", "69%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithGpsExpected(current: "62.0", prev: "61.2", prev2: "59.6")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithGpsExpected(current: "77.2", prev: "76.4", prev2: "74.5")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "69.0", prev: "67.5", prev2: "67.1")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "60%", "61%", "62%"],
            ["Test School 2", "75%", "76%", "77%"],
            ["Schools in England average", "67%", "68%", "69%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("expected-gps-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("expected-gps-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("expected-gps-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("expected-gps-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task AchievedHigherStandardGps_MeasureExistsOnPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var heading = page.ElementWithTestIdShouldExist("higher-gps-heading");
        heading.TrimmedTextContent().Should().Be("Achieved a higher standard in grammar, punctuation and spelling");

        var section = heading.Closest(".app-measure-section");
        section.Should().NotBeNull();

        var details = section!.QuerySelector("details.govuk-details");
        details.Should().NotBeNull();
        details!.QuerySelector(".govuk-details__summary-text")!.TrimmedTextContent()
            .Should().Be("Information about achieving the higher standard");
        details.HasAttribute("open").Should().BeFalse();
    }

    [Fact]
    public async Task AchievedHigherStandardGps_Tabs()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var tabs = page.ElementWithTestIdShouldExist("expected-gps-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TableView_ShouldShowCorrectValues()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "15", prev: "14", prev2: "13")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "16%", "17%", "18%"],
            ["Test School 2", "22%", "23%", "24%"],
            ["Schools in England average", "13%", "14%", "15%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithGpsHigher(current: "18.1", prev: "17.2", prev2: "15.5")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithGpsHigher(current: "24.0", prev: "23.2", prev2: "21.8")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "15.1", prev: "13.6", prev2: "13.2")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "16%", "17%", "18%"],
            ["Test School 2", "22%", "23%", "24%"],
            ["Schools in England average", "13%", "14%", "15%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ChartSettings()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("higher-gps-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("higher-gps-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_Charts_UseCorrectSchoolColours()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).Comparison(SimilarSchoolUrn).Ks2);

        var currentYearChart = page.ElementWithTestIdShouldExist("higher-gps-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("higher-gps-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }
}
