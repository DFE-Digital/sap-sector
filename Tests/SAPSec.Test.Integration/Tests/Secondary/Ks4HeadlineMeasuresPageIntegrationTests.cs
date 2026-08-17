using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class Ks4HeadlineMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Ks4HeadlineMeasures_WithNonExistentUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("999999").KS4HeadlineMeasures);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Progress8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("progress8-heading");
        heading.TrimmedTextContent().Should().Be("Progress 8");
    }

    [Fact]
    public async Task Attainment8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("attainment8-heading");
        heading.TrimmedTextContent().Should().Be("Attainment 8");
    }

    [Fact]
    public async Task Attainment8_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "107.4", prev: "106.6", prev2: "105.8")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithAttainment8(current: "104.5", prev: "103.5", prev2: "102.5")));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "103.2", prev: "102.2", prev2: "101.2")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "105.2", prev: "104.2", prev2: "103.2")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("attainment8-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Similar schools average", "102.2", "103.2", "104.2"],
            ["Local authority schools average", "102.5", "103.5", "104.5"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task Attainment8_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks4Performance.Establishment("100005", x => x.WithAttainment8(current: "103.7", prev: "102.9", prev2: "101.9")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("attainment8-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "106.3"],
            ["2", "Test School 2", "104.2"],
            ["3", "Test School 3", "104.2"]);
    }

    [Fact]
    public async Task Attainment8_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks4Performance.Establishment("100005", x => x.WithAttainment8(current: "103.7", prev: "102.9", prev2: "101.9")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("attainment8-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.SecondarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("attainment8-top-performers-table");
        var topPerformersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerformersLinks.Should().BeEquivalentTo([
            Routes.SecondarySchool("100001").Comparison("100004").Overview,
            Routes.SecondarySchool("100001").Comparison("100002").Overview,
            Routes.SecondarySchool("100001").Comparison("100003").Overview
        ]);
    }

    [Fact]
    public async Task Attainment8_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

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
    public async Task EnglishMaths_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("eng-maths-heading");
        heading.TrimmedTextContent().Should().Be("Grade achieved in English and maths GCSEs");
    }

    [Fact]
    public async Task EnglishMaths_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "91", prev: "90", prev2: "89")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishMaths_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "71.1", prev: "70.2", prev2: "69.3")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "91.02", prev: "89.7", prev2: "89.1")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngMaths49(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "22%"],
            ["2", "Test School 3", "21%"],
            ["3", "Test School 2", "20%"]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngMaths49(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("eng-maths-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.SecondarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.SecondarySchool("100001").Comparison("100004").Overview,
            Routes.SecondarySchool("100001").Comparison("100003").Overview,
            Routes.SecondarySchool("100001").Comparison("100002").Overview
        ]);
    }

    [Fact]
    public async Task EnglishMaths_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

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
    public async Task EnglishMaths_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("eng-maths-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "71%", "72%", "73%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "61%", "62%", "63%" }, new[] { "62%", "63%", "64%" })]
    [Theory]
    public async Task EnglishMaths_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] la, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "60", prev: "59", prev2: "58")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngMaths49(current: "70", prev: "69", prev2: "68")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngMaths49(current: "73", prev: "72", prev2: "71")
                .WithEngMaths59(current: "63", prev: "62", prev2: "61")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "74", prev: "73", prev2: "72")
                .WithEngMaths59(current: "64", prev: "63", prev2: "62")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);
        this.OutputHelper.WriteLine(page.DocumentElement.InnerHtml);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("eng-maths-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("eng-maths-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Similar schools average", .. similarSchools],
            ["Local authority schools average", .. la],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Destinations_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("destinations-heading");
        heading.TrimmedTextContent().Should().Be("Staying in education or entering employment");
    }

    [Fact]
    public async Task Destinations_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks4DestinationsRepository.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "91", prev: "90", prev2: "89")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Destinations_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "71.1", prev: "70.2", prev2: "69.3")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks4DestinationsRepository.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "91.02", prev: "89.7", prev2: "89.1")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Destinations_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Destinations.Establishment("100005", x => x.WithAllDest(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2022 to 2023"],
            ["1", "Test School 4", "22%"],
            ["2", "Test School 3", "21%"],
            ["3", "Test School 2", "20%"]);
    }

    [Fact]
    public async Task Destinations_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Destinations.Establishment("100005", x => x.WithAllDest(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("destinations-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.SecondarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.SecondarySchool("100001").Comparison("100004").Overview,
            Routes.SecondarySchool("100001").Comparison("100003").Overview,
            Routes.SecondarySchool("100001").Comparison("100002").Overview
        ]);
    }

    [Fact]
    public async Task Destinations_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

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
    public async Task Destinations_DestinationFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("destinations-dest-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["All destinations", "Education", "Employment and apprenticeships"]);
    }

    [InlineData("Education", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "71%", "72%", "73%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Employment and apprenticeships", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "61%", "62%", "63%" }, new[] { "62%", "63%", "64%" })]
    [Theory]
    public async Task Destinations_DestinationFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] la, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary().InLA("003")));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4DestinationsRepository.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "60", prev: "59", prev2: "58")),
            Build.Ks4Destinations.Establishment("100003", x => x
                .WithEducation(current: "70", prev: "69", prev2: "68")
                .WithEmployment(current: "62", prev: "61", prev2: "60")));

        Fixture.Ks4DestinationsRepository.SetupLADestinations(
             Build.Ks4Destinations.LA("001", x => x
                .WithEducation(current: "73", prev: "72", prev2: "71")
                .WithEmployment(current: "63", prev: "62", prev2: "61")));

        Fixture.Ks4DestinationsRepository.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithEducation(current: "74", prev: "73", prev2: "72")
                .WithEmployment(current: "64", prev: "63", prev2: "62")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").KS4HeadlineMeasures, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("destinations-dest-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("destinations-dest-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("destinations-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2020 to 2021", "2021 to 2022", "2022 to 2023"],
            ["Test School 1", .. currentSchool],
            ["Similar schools average", .. similarSchools],
            ["Local authority schools average", .. la],
            ["Schools in England average", .. england]);
    }
}
