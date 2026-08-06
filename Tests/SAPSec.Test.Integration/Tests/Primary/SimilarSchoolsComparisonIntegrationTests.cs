using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class SimilarSchoolsComparisonIntegrationTests(
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
    public async Task SimilarSchoolComparison_SchoolDetails_HeadingAndTitle_ReflectComparisonPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonSchoolDetails(SimilarSchoolUrn));

        page.Title.Should().Be("School details compared to Test School 2 - Get school improvement insights - GOV.UK");

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading.TrimmedTextContent().Should().Be("Test School 2");

        var caption = page.QuerySelector(".govuk-caption-xl");
        caption.Should().NotBeNull();
        caption.TrimmedTextContent().Should().Be("Test School 1");
    }

    [Fact]
    public async Task SimilarSchoolComparison_Ks2_DisplaysProgressScoreSection()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var heading = page.ElementWithTestIdShouldExist("expected-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Meeting expected standard in reading, writing and maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about meeting the expected standard");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "62%", "61%", "60%"],
            ["Schools in England average", "59%", "60%", "61%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var heading = page.ElementWithTestIdShouldExist("higher-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Achieved a higher standard in reading, writing and maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about achieving the higher standard");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();
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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "29%", "30%", "31%"],
            ["Test School 2", "22%", "21%", "20%"],
            ["Schools in England average", "19%", "20%", "21%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

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
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

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
}
