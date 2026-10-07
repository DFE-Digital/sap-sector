using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Text;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
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
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task Attendance_WhenAllThroughFeatureFlagDisabled_ReturnsNotFound()
    {
        SetupAllThroughSchool();
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, false);

        await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.NotFound);
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

        page.QuerySelectorAll("[data-testid$='-attendance-heading']")
            .Select(x => x.TrimmedTextContent())
            .Should().Equal("Primary attendance", "Secondary attendance");
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

    [Fact]
    public async Task Attendance_TableViews_ValuesRoundTo2DecimalPlaces()
    {
        SetupAllThroughSchool();

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment(Urn, x => x.WithOverallAbsence(current: "8.1052", previous: "8.315", previous2: "7.8923")));

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "9.102", previous: "8.975", previous2: "8.914")
                .WithOverallAbsenceSecondary(current: "10.104", previous: "9.995", previous2: "9.876")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "7.205", previous: "8.524", previous2: "9.495")
                .WithOverallAbsenceSecondary(current: "6.115", previous: "6.004", previous2: "5.896")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var primaryTable = page.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "7.89%", "8.32%", "8.11%"],
            ["Local authority primary schools average", "8.91%", "8.98%", "9.10%"],
            ["Primary schools in England average", "9.50%", "8.52%", "7.21%"]);

        var secondaryTable = page.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "7.89%", "8.32%", "8.11%"],
            ["Local authority secondary schools average", "9.88%", "10.00%", "10.10%"],
            ["Secondary schools in England average", "5.90%", "6.00%", "6.12%"]);
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("secondary")]
    public async Task Attendance_OverallAbsence_ChartSettings(string prefix)
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist($"{prefix}-absence-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "1"),
            ("axis-max", "10"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));

        var yearByYearChart = page.ElementWithTestIdShouldExist($"{prefix}-absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "1"),
            ("axis-max", "10"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("secondary")]
    public async Task Attendance_PersistentAbsence_ChartSettings(string prefix)
    {
        SetupAllThroughSchool();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>($"{prefix}-absence-type-filter");
        filter.SelectOption("Persistent absence");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>($"{prefix}-absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var currentYearChart = newPage.ElementWithTestIdShouldExist($"{prefix}-absence-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "5"),
            ("axis-max", "30"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));

        var yearByYearChart = newPage.ElementWithTestIdShouldExist($"{prefix}-absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "5"),
            ("axis-max", "30"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "2"),
            ("tooltip-decimals", "2"));
    }

    [Fact]
    public async Task Attendance_CharacteristicFilter_PrimarySection_UpdatesPrimaryTableOnly()
    {
        SetupAllThroughSchool();
        SetupBoysAndAllPupilsOverallAbsence();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("primary-absence-characteristic-filter");
        filter.SelectOption("Boys");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("primary-absence-characteristic-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var primaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "7.30%", "7.40%", "7.51%"],
            ["Local authority primary schools average", "5.40%", "5.50%", "5.62%"],
            ["Primary schools in England average", "4.80%", "4.90%", "5.03%"]);

        var secondaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.70%", "6.80%", "6.91%"],
            ["Local authority secondary schools average", "6.30%", "6.40%", "6.57%"],
            ["Secondary schools in England average", "5.90%", "6.00%", "6.11%"]);
    }

    [Fact]
    public async Task Attendance_CharacteristicFilter_SecondarySection_UpdatesSecondaryTableOnly()
    {
        SetupAllThroughSchool();
        SetupBoysAndAllPupilsOverallAbsence();

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("secondary-absence-characteristic-filter");
        filter.SelectOption("Boys");

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("secondary-absence-characteristic-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var secondaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("secondary-absence-table-view-table");
        secondaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "7.30%", "7.40%", "7.51%"],
            ["Local authority secondary schools average", "6.80%", "6.90%", "7.07%"],
            ["Secondary schools in England average", "6.40%", "6.50%", "6.61%"]);

        var primaryTable = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("primary-absence-table-view-table");
        primaryTable.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "6.70%", "6.80%", "6.91%"],
            ["Local authority primary schools average", "4.90%", "5.00%", "5.12%"],
            ["Primary schools in England average", "4.60%", "4.70%", "4.83%"]);
    }

    [Fact]
    public async Task Attendance_FiltersChangedOnBothSections_AreAppliedIndependentlyOnSubmit()
    {
        SetupAllThroughSchool();

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithPersistentAbsencePrimary(current: "20.00", previous: "19.50", previous2: "19.00")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")
                .WithOverallAbsenceGirlsSecondary(current: "6.80", previous: "6.70", previous2: "6.60")));

        var page = await Fixture.RequestPageAsync(Routes.AllThroughSchool(Urn).Attendance, HttpStatusCode.OK);

        page.ElementWithTestIdShouldExist<IHtmlSelectElement>("primary-absence-type-filter").SelectOption("Persistent absence");
        page.ElementWithTestIdShouldExist<IHtmlSelectElement>("secondary-absence-characteristic-filter").SelectOption("Girls");

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
            ["Local authority secondary schools average", "6.60%", "6.70%", "6.80%"],
            ["Secondary schools in England average", "No available data", "No available data", "No available data"]);
    }

    private static void AssertYearByYearChartPointStyles(IHtmlElement yearByYearChart, params string[] pointStyles)
    {
        var chartData = yearByYearChart.Dataset.Should().ContainKey("chart").WhoseValue;

        foreach (var pointStyle in pointStyles)
        {
            chartData.Should().Contain($"\"pointStyle\":\"{pointStyle}\"");
        }
    }

    private void SetupBoysAndAllPupilsOverallAbsence()
    {
        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment(Urn, x => x
                .WithOverallAbsence(current: "6.91", previous: "6.80", previous2: "6.70")
                .WithOverallAbsenceBoys(current: "7.51", previous: "7.40", previous2: "7.30")));

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceBoysPrimary(current: "5.62", previous: "5.50", previous2: "5.40")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")
                .WithOverallAbsenceBoysSecondary(current: "7.07", previous: "6.90", previous2: "6.80")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "4.83", previous: "4.70", previous2: "4.60")
                .WithOverallAbsenceBoysPrimary(current: "5.03", previous: "4.90", previous2: "4.80")
                .WithOverallAbsenceSecondary(current: "6.11", previous: "6.00", previous2: "5.90")
                .WithOverallAbsenceBoysSecondary(current: "6.61", previous: "6.50", previous2: "6.40")));
    }

    private void SetupAllThroughSchool()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().AllThrough().InLA("001")));
    }
}
