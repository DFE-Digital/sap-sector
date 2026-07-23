using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class Ks2PerformanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Progress8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("progress-rwm-heading");
        heading.ShouldHaveTextContent("Progress score in reading, writing and maths");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("expected-rwm-heading");
        heading.ShouldHaveTextContent("Meeting expected standard in reading, writing and maths");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91", prev: "90", prev2: "89")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91.02", prev: "89.7", prev2: "89.1")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71.1", prev: "70.2", prev2: "69.3")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "22%"],
            ["2", "Test School 3", "21%"],
            ["3", "Test School 2", "20%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("expected-rwm-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002")
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("reading-score-heading");
        heading.ShouldHaveTextContent("Average scaled score in reading");
    }

    [Fact]
    public async Task AverageScaledScoreReading_ChartsUseScaledScoreAxis()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var response = await Fixture.Client.GetAsync(Routes.PrimarySchool("100001").KS2);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        GetCanvasMarkup(content, "reading-score-school-chart").Should().ContainAll(
            "data-axis-min=\"80\"",
            "data-axis-step=\"20\"",
            "data-axis-max=\"120\"");
        GetCanvasMarkup(content, "reading-score-school-yearbyyear-chart").Should().ContainAll(
            "data-axis-min=\"80\"",
            "data-axis-step=\"5\"",
            "data-axis-max=\"120\"");
    }

    [Fact]
    public async Task AverageScaledScoreReading_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.4", prev: "106.6", prev2: "105.8")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithReadingScaledScore(current: "104.5", prev: "103.5", prev2: "102.5")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "103.2", prev: "102.2", prev2: "101.2")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "105.2", prev: "104.2", prev2: "103.2")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Similar schools average", "102.2", "103.2", "104.2"],
            ["Local authority schools average", "102.5", "103.5", "104.5"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithReadingScaledScore(current: "103.7", prev: "102.9", prev2: "101.9")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "106.3"],
            ["2", "Test School 2", "104.2"],
            ["3", "Test School 3", "104.2"]);
    }

    private static string GetCanvasMarkup(string content, string id)
    {
        var pattern = $"""<canvas[^>]*id="{Regex.Escape(id)}"[^>]*>""";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);

        match.Success.Should().BeTrue($"expected canvas '{id}' to be rendered");
        return match.Value;
    }

}
