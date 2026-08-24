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

public class ComparisonKs4HeadlineMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Ks4HeadlineMeasures_WithNonExistentUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("999999").Comparison("100002").KS4HeadlineMeasures);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_WithNonExistentSimilarSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("100001").Comparison("999999").KS4HeadlineMeasures);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Progress8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("progress8-heading");
        heading.TrimmedTextContent().Should().Be("Progress 8");
    }

    [Fact]
    public async Task Attainment8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("attainment8-heading");
        heading.TrimmedTextContent().Should().Be("Attainment 8");
    }

    [Fact]
    public async Task Attainment8_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "107.4", prev: "106.6", prev2: "105.8")));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.2", prev2: "102.2")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("attainment8-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Test School 2", "102.2", "103.2", "104.2"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task Attainment8_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("attainment8-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "30"),
            ("axis-max", "90"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("attainment8-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "30"),
            ("axis-max", "90"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "1"),
            ("tooltip-decimals", "1"));
    }

    [Fact]
    public async Task Attainment8_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("attainment8-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("attainment8-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task EnglishMaths_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("eng-maths-heading");
        heading.TrimmedTextContent().Should().Be("Grade achieved in English and maths GCSEs");
    }

    [Fact]
    public async Task EnglishMaths_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishMaths_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "91.02", prev: "89.7", prev2: "89.1")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishMaths_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-maths-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-maths-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task EnglishMaths_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-maths-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-maths-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task EnglishMaths_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("eng-maths-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [Theory]
    public async Task EnglishMaths_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "71", prev: "70", prev2: "69")
                .WithEngMaths59(current: "61", prev: "60", prev2: "59")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "74", prev: "73", prev2: "72")
                .WithEngMaths59(current: "64", prev: "63", prev2: "62")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);
        this.OutputHelper.WriteLine(page.DocumentElement.InnerHtml);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("eng-maths-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("eng-maths-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Destinations_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("destinations-heading");
        heading.TrimmedTextContent().Should().Be("Staying in education or entering employment");
    }

    [Fact]
    public async Task Destinations_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Destinations_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Destinations_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("destinations-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("destinations-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
    }

    [Fact]
    public async Task Destinations_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("destinations-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("destinations-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task Destinations_DestinationFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("destinations-dest-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["All destinations", "Education", "Employment and apprenticeships"]);
    }

    [InlineData("Education", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Employment and apprenticeships", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [Theory]
    public async Task Destinations_DestinationFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithEducation(current: "71", prev: "70", prev2: "69")
                .WithEmployment(current: "61", prev: "60", prev2: "59")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithEducation(current: "74", prev: "73", prev2: "72")
                .WithEmployment(current: "64", prev: "63", prev2: "62")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("destinations-dest-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("destinations-dest-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }
}
