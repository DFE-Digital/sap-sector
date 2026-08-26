using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Services.Helper;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class SchoolAttendanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Absence_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("attendance-heading");
        heading.TrimmedTextContent().Should().Be("Attendance");
    }

    [Fact]
    public async Task Absence_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.10", previous: "8.20", previous2: "7.90")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "10.10", previous: "10.00", previous2: "9.90")));

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsenceSecondary(current: "9.10", previous: "9.00", previous2: "8.90")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2021 to 2022", "2022 to 2023", "2023 to 2024"],
            ["Test School 1", "8.10%", "8.20%", "7.90%"],
            ["Local authority schools average", "9.10%", "9.00%", "8.90%"],
            ["Schools in England average", "10.10%", "10.00%", "9.90%"]);
    }

    [Fact]
    public async Task Absence_TableView_ValuesRoundTo2DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.1052", previous: "8.315", previous2: "7.8923")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "7.205", previous: "8.524", previous2: "9.495")));

        Fixture.AbsenceRepository.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsenceSecondary(current: "9.102", previous: "8.975", previous2: "8.914")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2021 to 2022", "2022 to 2023", "2023 to 2024"],
            ["Test School 1", "8.11%", "8.32%", "7.89%"],
            ["Local authority schools average", "9.10%", "8.98%", "8.91%"],
            ["Schools in England average", "7.21%", "8.52%", "9.50%"]);
    }

    [Fact]
    public async Task Absence_OverallAbsence_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

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
    }

    [Fact]
    public async Task Absence_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("absence-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("absence-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#5694ca", "#4b9b7d");
    }

    [Fact]
    public async Task Absence_TypeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("absence-type-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Overall absence", "Persistent absence"]);
    }

    [InlineData("Overall absence", new[] { "7.20%", "7.10%", "7.00%" }, new[] { "7.30%", "7.20%", "7.10%" }, new[] { "7.10%", "7.20%", "7.30%" })]
    [InlineData("Persistent absence", new[] { "6.00%", "6.10%", "6.20%" }, new[] { "5.90%", "6.00%", "6.10%" }, new[] { "6.10%", "6.20%", "6.30%" })]
    [Theory]
    public async Task Absence_TypeFilter_UpdatesTableViewWithTypeValues(string filterOption, string[] currentSchool, string[] la, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "7.20", previous: "7.10", previous2: "7.00")
                .WithPersistentAbsence(current: "6.00", previous: "6.10", previous2: "6.20")));

        Fixture.AbsenceRepository.SetupLAAbsence(
             Build.Absence.LA("001", x => x
                .WithOverallAbsenceSecondary(current: "7.30", previous: "7.20", previous2: "7.10")
                .WithPersistentAbsenceSecondary(current: "5.90", previous: "6.00", previous2: "6.10")));

        Fixture.AbsenceRepository.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsenceSecondary(current: "7.10", previous: "7.20", previous2: "7.30")
                .WithPersistentAbsenceSecondary(current: "6.10", previous: "6.20", previous2: "6.30")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("absence-type-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("absence-type-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("absence-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2021 to 2022", "2022 to 2023", "2023 to 2024"],
            ["Test School 1", .. currentSchool],
            ["Local authority schools average", .. la],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Absence_PersistentAbsence_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Attendance, HttpStatusCode.OK);

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
    }
}