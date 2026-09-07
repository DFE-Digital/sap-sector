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

public class ComparisonKs4CoreSubjectsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Ks4CoreSubjects_WithNonExistentUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("999999").Comparison("100002").KS4CoreSubjects);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ks4CoreSubjects_WithNonExistentSimilarSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("100001").Comparison("999999").KS4CoreSubjects);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnglishLanguage_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("eng-lang-heading");
        heading.TrimmedTextContent().Should().Be("English language");
    }

    [Fact]
    public async Task EnglishLanguage_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("eng-lang-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task EnglishLanguage_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lang-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishLanguage_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lang-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishLanguage_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-lang-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-lang-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task EnglishLanguage_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-lang-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-lang-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task EnglishLanguage_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "72", prev: "71", prev2: "70")
                .WithEngLang59(current: "62", prev: "61", prev2: "60")
                .WithEngLang79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "71", prev: "70", prev2: "69")
                .WithEngLang59(current: "61", prev: "60", prev2: "59")
                .WithEngLang79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLang49(current: "74", prev: "73", prev2: "72")
                .WithEngLang59(current: "64", prev: "63", prev2: "62")
                .WithEngLang79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);
        this.OutputHelper.WriteLine(page.DocumentElement.InnerHtml);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("eng-lang-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("eng-lang-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lang-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task EnglishLiterature_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("eng-lit-heading");
        heading.TrimmedTextContent().Should().Be("English literature");
    }

    [Fact]
    public async Task EnglishLiterature_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("eng-lit-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task EnglishLiterature_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks4PerformanceRepository.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLit49(current: "91", prev: "90", prev2: "89")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lit-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishLiterature_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lit-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task EnglishLiterature_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-lit-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-lit-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task EnglishLiterature_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("eng-lang-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("eng-lang-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task EnglishLiterature_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("eng-lit-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task EnglishLiterature_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "72", prev: "71", prev2: "70")
                .WithEngLit59(current: "62", prev: "61", prev2: "60")
                .WithEngLit79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "71", prev: "70", prev2: "69")
                .WithEngLit59(current: "61", prev: "60", prev2: "59")
                .WithEngLit79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLit49(current: "74", prev: "73", prev2: "72")
                .WithEngLit59(current: "64", prev: "63", prev2: "62")
                .WithEngLit79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);
        this.OutputHelper.WriteLine(page.DocumentElement.InnerHtml);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("eng-lit-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("eng-lit-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("eng-lit-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Maths_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("maths-heading");
        heading.TrimmedTextContent().Should().Be("Maths");
    }

    [Fact]
    public async Task Maths_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("maths-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Maths_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Maths_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Maths_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("maths-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("maths-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Maths_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("maths-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("maths-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task Maths_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("maths-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task Maths_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "72", prev: "71", prev2: "70")
                .WithMaths59(current: "62", prev: "61", prev2: "60")
                .WithMaths79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "71", prev: "70", prev2: "69")
                .WithMaths59(current: "61", prev: "60", prev2: "59")
                .WithMaths79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithMaths49(current: "74", prev: "73", prev2: "72")
                .WithMaths59(current: "64", prev: "63", prev2: "62")
                .WithMaths79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("maths-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("maths-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task CombinedScience_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("comb-sci-heading");
        heading.TrimmedTextContent().Should().Be("Combined science (double award)");
    }

    [Fact]
    public async Task CombinedScience_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("comb-sci-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task CombinedScience_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("comb-sci-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task CombinedScience_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("comb-sci-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task CombinedScience_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("comb-sci-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("comb-sci-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task CombinedScience_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("comb-sci-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("comb-sci-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task CombinedScience_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("comb-sci-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4-4 and above", "Grade 5-5 and above", "Grade 7-7 and above"]);
    }

    [InlineData("Grade 4-4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5-5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7-7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task CombinedScience_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "72", prev: "71", prev2: "70")
                .WithCombSci59(current: "62", prev: "61", prev2: "60")
                .WithCombSci79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "71", prev: "70", prev2: "69")
                .WithCombSci59(current: "61", prev: "60", prev2: "59")
                .WithCombSci79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithCombSci49(current: "74", prev: "73", prev2: "72")
                .WithCombSci59(current: "64", prev: "63", prev2: "62")
                .WithCombSci79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);
        this.OutputHelper.WriteLine(page.DocumentElement.InnerHtml);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("comb-sci-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("comb-sci-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("comb-sci-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Biology_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("bio-heading");
        heading.TrimmedTextContent().Should().Be("Biology");
    }

    [Fact]
    public async Task Biology_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("bio-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Biology_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "71", prev: "70", prev2: "69")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("bio-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Biology_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "71.1", prev: "70.2", prev2: "69.3")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("bio-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Biology_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("bio-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("bio-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Biology_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("bio-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("bio-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task Biology_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("bio-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task Biology_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "72", prev: "71", prev2: "70")
                .WithBio59(current: "62", prev: "61", prev2: "60")
                .WithBio79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "71", prev: "70", prev2: "69")
                .WithBio59(current: "61", prev: "60", prev2: "59")
                .WithBio79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithBio49(current: "74", prev: "73", prev2: "72")
                .WithBio59(current: "64", prev: "63", prev2: "62")
                .WithBio79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("bio-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("bio-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("bio-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Chemistry_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("chem-heading");
        heading.TrimmedTextContent().Should().Be("Chemistry");
    }

    [Fact]
    public async Task Chemistry_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("chem-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Chemistry_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("chem-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Chemistry_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("chem-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Chemistry_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("chem-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("chem-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Chemistry_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("chem-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("chem-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task Chemistry_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("chem-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task Chemistry_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "72", prev: "71", prev2: "70")
                .WithChem59(current: "62", prev: "61", prev2: "60")
                .WithChem79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "71", prev: "70", prev2: "69")
                .WithChem59(current: "61", prev: "60", prev2: "59")
                .WithChem79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithChem49(current: "74", prev: "73", prev2: "72")
                .WithChem59(current: "64", prev: "63", prev2: "62")
                .WithChem79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("chem-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("chem-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("chem-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task Physics_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("phys-heading");
        heading.TrimmedTextContent().Should().Be("Physics");
    }

    [Fact]
    public async Task Physics_Tabs()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var tabs = page.ElementWithTestIdShouldExist("phys-tabs");
        tabs.ChildTrimmedTextContent().Should().BeEquivalentTo("Charts", "Table");
    }

    [Fact]
    public async Task Physics_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "101", prev: "100", prev2: "99")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("phys-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Physics_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "70.6", prev: "70.3", prev2: "69.1")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "101.31", prev: "99.52", prev2: "99.49")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("phys-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Test School 2", "69%", "70%", "71%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task Physics_ChartSettings()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("phys-current-year-chart");
        currentYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));

        var yearByYearChart = page.ElementWithTestIdShouldExist("phys-year-by-year-chart");
        yearByYearChart.Dataset.Should().Contain(
            ("axis-min", "0"),
            ("axis-step", "25"),
            ("axis-max", "100"),
            ("axis-auto-skip", "false"),
            ("label-decimals", "0"),
            ("tooltip-decimals", "0"));
        AssertYearByYearChartPointStyles(yearByYearChart, "triangle", "circle", "rectRot");
    }

    [Fact]
    public async Task Physics_Charts_UseCorrectSchoolColours()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var currentYearChart = page.ElementWithTestIdShouldExist("phys-current-year-chart");
        currentYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#2a1950");

        var yearByYearChart = page.ElementWithTestIdShouldExist("phys-year-by-year-chart");
        yearByYearChart.Dataset.Should().ContainKey("colors")
            .WhoseValue.DeserializeToList<string>().Should().BeEquivalentTo("#ca357c", "#2a1950", "#4b9b7d");
    }

    [Fact]
    public async Task Physics_GradeFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("phys-grade-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Grade 4 and above", "Grade 5 and above", "Grade 7 and above"]);
    }

    [InlineData("Grade 4 and above", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Grade 5 and above", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Grade 7 and above", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task Physics_GradeFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Secondary()));

        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "72", prev: "71", prev2: "70")
                .WithPhysics59(current: "62", prev: "61", prev2: "60")
                .WithPhysics79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "71", prev: "70", prev2: "69")
                .WithPhysics59(current: "61", prev: "60", prev2: "59")
                .WithPhysics79(current: "51", prev: "50", prev2: "49")));

        Fixture.Ks4PerformanceRepository.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithPhysics49(current: "74", prev: "73", prev2: "72")
                .WithPhysics59(current: "64", prev: "63", prev2: "62")
                .WithPhysics79(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Comparison("100002").KS4CoreSubjects, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("phys-grade-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("phys-grade-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("phys-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Test School 2", .. similarSchools],
            ["Schools in England average", .. england]);
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
