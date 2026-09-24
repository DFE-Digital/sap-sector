using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Core.Services.Helper;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.AllThrough;

public class SchoolAttendanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Urn = "100001";

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnableAllThroughSchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task Attendance_ShowsPrimaryAndSecondaryHeadingsInOrder()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var primaryHeading = page.ElementWithTestIdShouldExist("primary-attendance-heading");
        primaryHeading.TrimmedTextContent().Should().Be("Primary attendance");

        var secondaryHeading = page.ElementWithTestIdShouldExist("secondary-attendance-heading");
        secondaryHeading.TrimmedTextContent().Should().Be("Secondary attendance");
    }

    [Fact]
    public async Task Attendance_BothSections_HaveChartsAndTableTabsOnly()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        page.ElementWithTestIdShouldExist("primary-absence-tabs").ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
        page.ElementWithTestIdShouldExist("secondary-absence-tabs").ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Attendance_BothSections_HaveIndependentFilterDropdowns()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        page.ElementWithTestIdShouldExist("primary-absence-type-filter").ChildTrimmedTextContent()
            .Should().Equal(["Overall absence", "Persistent absence"]);
        page.ElementWithTestIdShouldExist("primary-absence-characteristic-filter").ChildTrimmedTextContent()
            .Should().Equal([
                "All pupils", "Boys", "Girls", "Ever 6 FSM pupils", "Non-Ever 6 FSM pupils",
                "English as an additional language", "English as a first language"
            ]);

        page.ElementWithTestIdShouldExist("secondary-absence-type-filter").ChildTrimmedTextContent()
            .Should().Equal(["Overall absence", "Persistent absence"]);
        page.ElementWithTestIdShouldExist("secondary-absence-characteristic-filter").ChildTrimmedTextContent()
            .Should().Equal([
                "All pupils", "Boys", "Girls", "Ever 6 FSM pupils", "Non-Ever 6 FSM pupils",
                "English as an additional language", "English as a first language"
            ]);
    }

    [Fact]
    public async Task Attendance_TableViews_ShowCorrectValuesAndPhaseQualifiedLabels()
    {
        SetupAllThroughSchool();

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment(Urn, x => x.WithOverallAbsence(current: "6.91", previous: "6.80", previous2: "6.70")));

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "4.83", previous: "4.70", previous2: "4.60")
                .WithOverallAbsenceSecondary(current: "6.11", previous: "6.00", previous2: "5.90")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var primaryTable = page.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.70%", "6.80%", "6.91%"],
            ["Local authority primary schools average", "4.90%", "5.00%", "5.12%"],
            ["Primary schools in England average", "4.60%", "4.70%", "4.83%"]);

        var secondaryTable = page.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.70%", "6.80%", "6.91%"],
            ["Local authority secondary schools average", "6.30%", "6.40%", "6.57%"],
            ["Secondary schools in England average", "5.90%", "6.00%", "6.11%"]);
    }

    [Fact]
    public async Task Attendance_BothSections_UseCorrectChartColoursAndPointStyles()
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        foreach (var prefix in new[] { "primary", "secondary" })
        {
            var currentYearChart = page.ElementWithTestIdShouldExist($"{prefix}-absence-current-year-chart");
            currentYearChart.Dataset.Should().ContainKey("colors")
                .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

            var yearByYearChart = page.ElementWithTestIdShouldExist($"{prefix}-absence-year-by-year-chart");
            yearByYearChart.Dataset.Should().ContainKey("colors")
                .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#5694ca", "#4b9b7d");

            AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "rect", "rectRot");
        }
    }

    [Fact]
    public async Task Attendance_FilteringPrimarySection_DoesNotAffectSecondarySection()
    {
        SetupAllThroughSchool();

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithPersistentAbsencePrimary(current: "20.00", previous: "19.50", previous2: "19.00")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("primary-absence-type-filter");
        filter.SelectOption("Persistent absence");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("primary-absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var primaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "No available data", "No available data", "No available data"],
            ["Local authority primary schools average", "19.00%", "19.50%", "20.00%"],
            ["Primary schools in England average", "No available data", "No available data", "No available data"]);

        var secondaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "No available data", "No available data", "No available data"],
            ["Local authority secondary schools average", "6.30%", "6.40%", "6.57%"],
            ["Secondary schools in England average", "No available data", "No available data", "No available data"]);
    }

    [Fact]
    public async Task Attendance_FilteringSecondarySection_DoesNotAffectPrimarySection()
    {
        SetupAllThroughSchool();

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")
                .WithPersistentAbsenceSecondary(current: "25.00", previous: "24.50", previous2: "24.00")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("secondary-absence-type-filter");
        filter.SelectOption("Persistent absence");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("secondary-absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var secondaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "No available data", "No available data", "No available data"],
            ["Local authority secondary schools average", "24.00%", "24.50%", "25.00%"],
            ["Secondary schools in England average", "No available data", "No available data", "No available data"]);

        var primaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "No available data", "No available data", "No available data"],
            ["Local authority primary schools average", "4.90%", "5.00%", "5.12%"],
            ["Primary schools in England average", "No available data", "No available data", "No available data"]);
    }

    private static void AssertYearByYearChartPointStyles(IHtmlElement yearByYearChart, params string[] pointStyles)
    {
        var chartData = yearByYearChart.Dataset.Should().ContainKey("chart").WhoseValue;

        foreach (var pointStyle in pointStyles)
        {
            chartData.Should().Contain($"\"pointStyle\":\"{pointStyle}\"");
        }
    }

    private void SetupAllThroughSchool()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableAllThroughSchools, true);

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001")));
    }
}
