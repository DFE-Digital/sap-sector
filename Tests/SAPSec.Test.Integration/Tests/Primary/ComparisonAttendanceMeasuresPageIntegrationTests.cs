using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Core.Services.Helper;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonAttendanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task NonExistentCurrentSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("999999").Comparison("100002").Attendance, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NonExistentComparatorSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("999999").Attendance, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", []));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Absence_DisplaysHeadingAndFilterOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var heading = page.ElementWithTestIdShouldExist("attendance-heading");
        heading.TrimmedTextContent().Should().Be("Attendance");

        var filter = page.ElementWithTestIdShouldExist("absence-type-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Overall absence", "Persistent absence"]);
    }

    [Fact]
    public async Task Absence_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var tabs = page.ElementWithTestIdShouldExist("absence-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Absence_TableView_ShowsOverallAbsenceValuesByDefault()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2021 to 2022", "2022 to 2023", "2023 to 2024"],
            ["Current School", "7.91%", "8.05%", "8.00%"],
            ["Comparator School", "6.30%", "6.20%", "6.10%"],
            ["Schools in England average", "5.45%", "6.90%", "6.10%"]);
    }

    [Fact]
    public async Task Absence_FilterBy_Persistent_UpdatesTableViewWithPersistentAbsenceValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithPersistentAbsence(current: "2.27", previous: "1.24", previous2: "8.20")),
            Build.Absence.Establishment("100002", x => x
                .WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")
                .WithPersistentAbsence(current: "1.24", previous: "1.30", previous2: "1.40")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")
                .WithPersistentAbsencePrimary(current: "3.20", previous: "2.24", previous2: "2.20")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("absence-type-filter");
        filter.SelectOption("Persistent absence");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2021 to 2022", "2022 to 2023", "2023 to 2024"],
            ["Current School", "8.20%", "1.24%", "2.27%"],
            ["Comparator School", "1.40%", "1.30%", "1.24%"],
            ["Schools in England average", "2.20%", "2.24%", "3.20%"]);
    }

    [Fact]
    public async Task Absence_OverallAbsence_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var currentYearChart = page.ElementWithTestIdShouldExist("absence-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "1"),
            ("axis-max", "10"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "1"),
            ("axis-max", "10"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Absence_PersistentAbsence_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("absence-type-filter");
        filter.SelectOption("Persistent absence");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var currentYearChart = newPage.ElementWithTestIdShouldExist("absence-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "5"),
            ("axis-max", "30"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));

        var yearByYearChart = newPage.ElementWithTestIdShouldExist("absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "5"),
            ("axis-max", "30"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Absence_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Attendance);

        var currentYearChart = page.ElementWithTestIdShouldExist("absence-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    private static void AssertYearByYearChartPointStyles(IHtmlElement yearByYearChart, params string[] pointStyles)
    {
        var chartData = yearByYearChart.Dataset.Should().ContainKey("chart").WhoseValue;

        foreach (var pointStyle in pointStyles)
        {
            chartData.Should().Contain($"\"pointStyle\":\"{pointStyle}\"");
        }
    }
}
